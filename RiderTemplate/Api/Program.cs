using System;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Api.Filters;
using Api.Middleware;
using Api.Telemetry;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api
{
    public static class Program
    {
        private static ConfigurationManager _configuration;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            BuildConfiguration(builder.Configuration);
            CreateLogger(builder.Logging);
            ConfigureServices(builder.Services);

            var app = builder.Build();
            ConfigureApplication(app);

            app.Run();
        }

        private static void BuildConfiguration(ConfigurationManager configuration)
        {
            configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile
                (
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true
                )
                .AddJsonFile
                (
                    $"appsettings.{EnvironmentExtension.CurrentEnvironment}.json",
                    optional: true,
                    reloadOnChange: true
                )
                .Build();

            _configuration = configuration;
        }

        private static void CreateLogger(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => options.Filters.Add(typeof(ApiExceptionFilterAttribute)))
                .AddJsonOptions(
                    options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.WriteIndented = true;
                        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    });

            services.AddApiVersioning
                (
                    options =>
                    {
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        options.ReportApiVersions = true;
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                    }
                ).AddMvc();

            services.AddHttpContextAccessor();

            services.AddAuthorization();
            services.AddAuthentication("Bearer").AddJwtBearer();

            services.AddTracing(_configuration);

            services.AddApplication();

            services.ConfigureSettings(_configuration);

            services.AddInfrastructure(_configuration);

            services.AddCors();

            services.AddEndpointsApiExplorer();
            
            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Project",
                    Version = "v1"
                });

                setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                setup.OperationFilter<AuthorizationHeaderOperationHeader>();
                setup.OperationFilter<ApiVersionOperationFilter>();
            });

            services.AddOptions();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.Use(GetDurationHeaderSetterMiddlewareFunction());
            }

            app.UseCors
            (
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            );

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<RequestBodyBufferingMiddleware>();

            app.MapControllers();
        }

        private static Func<HttpContext, RequestDelegate, Task> GetDurationHeaderSetterMiddlewareFunction()
        {
            return async (context, next) =>
            {
                var watcher = new Stopwatch();
                context.Response.OnStarting
                (
                    innerWatcher =>
                    {
                        context.Response.Headers.Add("X-Response-Time", $"{((Stopwatch)innerWatcher).ElapsedMilliseconds}ms");
                        return Task.CompletedTask;
                    },
                    watcher
                );
                watcher.Start();
                await next(context);
                watcher.Stop();
            };
        }

        private static void AddAuthenticationAndAuthorization(WebApplicationBuilder webApplicationBuilder)
        {
            var jwtSection = webApplicationBuilder.Configuration
                .GetSection("JwtOptions");

            webApplicationBuilder.Services.AddAuthentication
                (
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddJwtBearer
                (
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        // Используем симетричное шифрование
                        // Валидируем только издателя и время жизни
                        // Валидация Audience отключена, т.к. на данный момент приложение является и издателем и единственным потребителем
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSection["SecretKey"])),
                            ValidIssuer = jwtSection["Issuer"],
                            ValidateAudience = false,
#if DEBUG
                            ValidateLifetime = false,
#else
                            ValidateLifetime = true,
#endif
                        };
                    }
                );
        }
    }
}
