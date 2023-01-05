using System;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
	        services.AddSingleton<ISingletonInterceptor, ExampleMaterializationInterceptor>();
	        //transient because by factory we can generate a lot of dbContexts in same scope
	        services.AddTransient<IInterceptor, ExampleConnectionDurationInterceptor>();
	        
            var defaultDatabaseConnection = configuration.GetConnectionString("DefaultConnection");
            services.AddEntityFrameworkSqlServer();
            services.AddPooledDbContextFactory<DatabaseContext>
			(
				(provider, options) =>
				{
					options.UseSqlServer
					(
						defaultDatabaseConnection, b =>
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

            services.AddScoped<IPooledDbContextFactory, PooledDbContextFactory>();
            services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<IPooledDbContextFactory>().CreateContext());

            return services;
        }
    }
}
