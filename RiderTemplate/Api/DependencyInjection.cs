using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
