using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var defaultDatabaseConnection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DatabaseContext>
            (
                options =>
                    options.UseSqlServer
                    (
                        defaultDatabaseConnection,
                        b => b.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName)
                    )
            );

            services.AddScoped<IDatabaseContext, DatabaseContext>();

            return services;
        }
    }
}