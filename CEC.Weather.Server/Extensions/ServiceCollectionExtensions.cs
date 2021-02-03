using CEC.Blazor.Extensions;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CEC.Blazor.Services;

namespace CEC.Blazor.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Singleton service for the Server Side version of WeatherForecast Data Service 
            // Dummy service produces a new recordset each time the application runs 

            services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, WeatherDummyDataService>();

            // services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, WeatherDummyDataService<WeatherForecastDbContext>>();

            //services.AddSingleton<SalaryDataService, SalaryDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            services.AddScoped<WeatherStationControllerService>();
            services.AddScoped<WeatherReportControllerService>();
            // services.AddScoped<SalaryControllerService>();
            // Factory for building the DBContext 
            var dbContext = configuration.GetValue<string>("Configuration:DBContext");
            services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
            return services;
        }

    }
}
