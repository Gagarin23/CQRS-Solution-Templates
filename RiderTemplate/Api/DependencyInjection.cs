using Application.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApplicationOptions>(configuration.GetSection(nameof(ApplicationOptions)));

            return services;
        }
    }
}
