using Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            
            services.AddMediatR(
                configuration =>
                {
                    configuration.Lifetime = ServiceLifetime.Transient;
                    configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
                    configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(InputValidationBehavior<,>));
                    configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(BusinessValidationBehavior<,>));
                    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                });

            return services;
        }
    }
}
