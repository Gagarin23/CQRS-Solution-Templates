using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Api.Filters;
using Api.Middleware;
using Application.Common.Validators;
using Castle.DynamicProxy;
using FluentValidation;
using HarmonyLib;
using Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Telemetry
{
    public class TracingConfig
    {
        public bool Enabled { get; set; }
        public string ServiceName { get; set; }
        public string JaegerEndpoint { get; set; }
        public string Protocol { get; set; }
        public bool CollectQueryTextEnabled { get; set; }
        public bool CollectHttpBodyEnabled { get; set; }
        public int HttpBodySizeLimitInBytes { get; set; }
    }

    public class TracingInterceptorConfig
    {
        public string SourceName { get; }
        public List<Tag> Tags { get; }

        public TracingInterceptorConfig(string sourceName)
        {
            SourceName = sourceName;
            Tags = new List<Tag>();
        }

        public class Tag
        {
            public string Name { get; }
            public string Value { get; }
            public Tag(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
    }

    public static class TracingExtensions
    {
        private static Dictionary<Type, TracingInterceptorConfig> Sources;
        private static PathString StartPathSegment = new PathString("/api");

        static TracingExtensions()
        {
            Sources = new Dictionary<Type, TracingInterceptorConfig>();

            //ищем интерфейсы в указанной сборке
            var serviceTypes = Assembly.GetAssembly(typeof(Application.AssemblyMark))
                .GetTypes()
                .Where(type => type.IsInterface);

            foreach (var serviceType in serviceTypes)
            {
                Sources.Add(serviceType, new TracingInterceptorConfig(serviceType.Name));
            }
        }

        public static IServiceCollection AddTracing(this IServiceCollection services, IConfiguration configuration)
        {
            var tracingSection = configuration.GetSection("Tracing");
            if (!tracingSection.Exists())
                return services;

            var config = new TracingConfig();
            tracingSection.Bind(config);
            services.Configure<TracingConfig>(tracingSection);

            AddInlineTracingForRequestHandlers();

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                    {
                        builder
                            //настройка телеметрии в рамках экземпляра приложения
                            .AddAspNetCoreInstrumentation
                            (
                                config =>
                                {
                                    config.RecordException = true;
                                    //фильтруем, чтобы в трассировку попадали только вызовы методов контроллеров,
                                    //к примеру, исключая статический контент.
                                    //Отрабатывает за микросекунды, но на всякий случай сделал таймаут
                                    config.Filter = context => context.Request.Path.StartsWithSegments(StartPathSegment);

                                    // включаем сбор тела запроса в Response т.к. активность запускается перед RequestBodyBufferingMiddleware
                                    config.EnrichWithHttpResponse = (activity, response) =>
                                    {
                                        try
                                        {
                                            var request = response.HttpContext.Request;
                                            if (request.HttpContext.Items.TryGetValue(RequestBodyBufferingMiddleware.RequestBodyKey, out var item)
                                                && item is byte[] bodyBytes)
                                            {
                                                var bodyString = Encoding.UTF8.GetString(bodyBytes);
                                                activity?.AddTag("body", bodyString);
                                            }
                                        }
                                        catch
                                        {
                                            //ignored
                                        }
                                    };
                                }
                            )
                            .AddHttpClientInstrumentation(config => { config.RecordException = true; })
                            .AddSqlClientInstrumentation
                            (
                                opt =>
                                {
                                    opt.RecordException = true;
                                    opt.SetDbStatementForText = config.CollectQueryTextEnabled;
                                    opt.SetDbStatementForStoredProcedure = true;
                                    opt.Filter = context =>
                                    {
                                        if (context is not SqlCommand command)
                                        {
                                            return false;
                                        }

                                        var commandText = command.CommandText;
                                        if (commandText == null || commandText.Length <= UserCodeDbCommandInterceptor.UserCodeQueryComment.Length)
                                        {
                                            return false;
                                        }

                                        return commandText.StartsWith(UserCodeDbCommandInterceptor.UserCodeQueryComment, StringComparison.Ordinal);
                                    };
                                }
                            )
                            .AddSource(Sources.Keys.Select(x => x.Name)
                                .Union(
                                    Assembly.GetAssembly(typeof(Application.AssemblyMark))
                                        .GetTypes()
                                        .Where(type =>
                                            type.GetInterfaces()
                                                .Any(@interface => @interface.IsGenericType
                                                                   && (@interface.GetGenericTypeDefinition() == typeof(IRequest<>)
                                                                        || @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                                                                   || @interface == typeof(INotification)))
                                        .Select(type => type.Name)
                                )
                                .Union(new []{nameof(ApiExceptionFilterAttribute)})
                                .ToArray())
                            .SetResourceBuilder
                            (
                                ResourceBuilder
                                    .CreateDefault()
                                    .AddService(config.ServiceName)
                            );

                        builder.AddOtlpExporter
                        (
                            opt =>
                            {
                                opt.Endpoint = new Uri(config.JaegerEndpoint);
                                switch (config.Protocol)
                                {
                                    case "http":
                                        opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                                        break;

                                    case "grpc":
                                        opt.Protocol = OtlpExportProtocol.Grpc;
                                        break;

                                    default:
                                        throw new Exception("Protocol is missing");
                                }
                            }
                        );
                    }
                );
            
            AddInterceptedTracingForServices(services);

            return services;
        }

        /// <summary>
        /// Подменяем сервисные дескрипторы на прокси
        /// </summary>
        /// <param name="services"></param>
        private static void AddInterceptedTracingForServices(IServiceCollection services)
        {
            var concreteDescriptors = new List<ServiceDescriptor>();
            
            for (int i = 0; i < services.Count; i++)
            {
                var existDescriptor = services[i];

                if (!Sources.ContainsKey(existDescriptor.ServiceType))
                {
                    continue;
                }
                
                var generator = new ProxyGenerator();
                ServiceDescriptor newDescriptor;

                if (existDescriptor.ImplementationFactory != null)
                {
                    newDescriptor = CreateDescriptorFromImplementationFactory(existDescriptor, generator);
                }
                else if (existDescriptor.ImplementationType != null)
                {
                    newDescriptor = CreateDescriptorFromImplementationType(existDescriptor, generator);
                    
                    //чтобы получить оборачиваемый тип через провайдер внутри фабрики,
                    //мы должны зарегестрировать этот самый тип as is
                    concreteDescriptors.Add(CreateDescriptorForImplementationType(existDescriptor));
                }
                else
                {
                    //если Sources содержит тип, который зарегестрирован без интерфейса,
                    //то для такого типа мы не сможем написать универсальный метод,
                    //т.к. не зная аргументов конструктора, в фабрике мы можем получить экземпляр только через провайдер
                    //и по понятным причинам попадаем в бесконечный цикл.
                    //Такие типы следует регестрировать в ручную, помечая публичные методы как virtual
                    continue;
                }

                //подменяем дескриптор
                services[i] = newDescriptor;
            }

            foreach (var descriptor in concreteDescriptors)
            {
                services.Add(descriptor);
            }
        }

        private static ServiceDescriptor CreateDescriptorForImplementationType(ServiceDescriptor descriptor)
        {
            return new ServiceDescriptor
            (
                descriptor.ImplementationType,
                descriptor.ImplementationType,
                descriptor.Lifetime
            );
        }

        private static ServiceDescriptor CreateDescriptorFromImplementationType(ServiceDescriptor descriptor, ProxyGenerator generator)
        {
            return new ServiceDescriptor
            (
                descriptor.ServiceType,
                serviceProvider =>
                    generator.CreateInterfaceProxyWithTarget
                    (
                        descriptor.ServiceType,
                        serviceProvider.GetService(descriptor.ImplementationType),
                        new TelemetryInterceptor(Sources[descriptor.ServiceType],
                            serviceProvider.GetService<IHttpContextAccessor>())
                    ),
                descriptor.Lifetime
            );
        }

        private static ServiceDescriptor CreateDescriptorFromImplementationFactory(ServiceDescriptor descriptor, ProxyGenerator generator)
        {
            return new ServiceDescriptor
            (
                descriptor.ServiceType,
                serviceProvider =>
                {
                    var targetObject = descriptor.ImplementationFactory.Invoke(serviceProvider);
                    return generator.CreateInterfaceProxyWithTarget
                    (
                        descriptor.ServiceType,
                        targetObject,
                        new TelemetryInterceptor(Sources[descriptor.ServiceType],
                            serviceProvider.GetService<IHttpContextAccessor>())
                    );
                },
                descriptor.Lifetime
            );
        }

        /// <summary>
        /// Модификация IL кода для добавления трассировки в каждый метод обработчиков запросов
        /// </summary>
        private static void AddInlineTracingForRequestHandlers()
        {
            if (!bool.TryParse(Environment.GetEnvironmentVariable("UseDangerTracingIntercepting"), out var use) || !use)
            {
                return;
            }

            // Получаем все обработчики
            var allTypes = Assembly.GetAssembly(typeof(Application.AssemblyMark))
                .GetTypes()
                .Where(p => p.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                         || i.BaseType?.GetGenericTypeDefinition() == typeof(InputValidator<>)
                         || i.BaseType?.GetGenericTypeDefinition() == typeof(BusinessValidator<>)
                         || i.BaseType?.GetGenericTypeDefinition() == typeof(AbstractValidator<>)
                    ) && p.IsClass)
                )
                .ToList();

            var harmony = new Harmony(Assembly.GetAssembly(typeof(Application.AssemblyMark)).FullName);

            foreach (var type in allTypes)
            {
                // Отсеиваем точку входа в обработчик "Handle", т.к. этот метод вызывается через обобщённый делегат.
                // Дока harmony просит избегать таких кейсов.
                // + на ValueTuple<,> тоже ломаемся.
                var methods = AccessTools.GetDeclaredMethods(type)
                    .Where
                    (
                        x => x.ReturnType != typeof(void)
                             && !(x.ReturnType.IsGenericType && x.ReturnType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                             && x.GetParameters().All(p => !p.GetType().IsSubclassOf(typeof(Delegate)))
                             && (!x.ReturnParameter?.GetType().IsSubclassOf(typeof(Delegate)) ?? true)
                    );

                foreach (var method in methods)
                {
                    var prefix = typeof(DangerTracingInterceptor).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
                    var postfix = typeof(DangerTracingInterceptor).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);

                    harmony.Patch
                    (
                        method,
                        prefix: new HarmonyMethod(prefix),
                        postfix: new HarmonyMethod(postfix)
                    );

                }
            }
        }
    }
}
