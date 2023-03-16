using System;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var defaultDatabaseConnection = configuration.GetConnectionString("DefaultConnection");
            services.AddEntityFrameworkSqlServer();
            services.AddPooledDbContextFactory<DatabaseContext>
            (
                (provider, options) =>
                {
                    options.UseSqlServer
                    (
                        defaultDatabaseConnection,
                        b =>
                            b.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName)
                    );
                    options.UseInternalServiceProvider(provider);
                    if (EnvironmentExtension.IsDevelopment)
                    {
                        options.EnableSensitiveDataLogging();
                        options.LogTo(Console.WriteLine, LogLevel.Information);
                    }
                }
            );

            services.AddScoped<IDatabaseContext>
            (
                provider => provider.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext()
            );

            return services;
        }
    }
}
