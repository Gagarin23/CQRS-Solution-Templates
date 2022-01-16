using Api.Filters;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace Api
{
    public class Program
    {
        private static ConfigurationManager _configuration;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            BuildConfiguration(builder.Configuration);
            //CreateLogger(builder.Logging);
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
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true,
                    reloadOnChange: true
                )
                .Build();

            _configuration = configuration;
        }

        private static void CreateLogger(ILoggingBuilder loggingBuilder)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            loggingBuilder.AddSerilog(logger);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers
            (
                options =>
                    options.Filters.Add(new ApiExceptionFilter())
            );

            services.AddControllers();

            services.AddApplication();

            services.ConfigureSettings(_configuration);

            services.AddInfrastructure(_configuration);

            services.AddCors
            (
                options =>
                {
                    options.AddPolicy
                    (
                        "AllowAll",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                        }
                    );
                }
            );

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddOptions();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
