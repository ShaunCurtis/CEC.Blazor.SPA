using CEC.Blazor.Services;
using CEC.Weather.Data;
using CEC.Weather.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.WASM.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            // Scoped service for the WASM Client version of WASM Factory Data Service 
            services.AddScoped<IFactoryDataService<WeatherForecastDbContext>, FactoryWASMDataService<WeatherForecastDbContext>>();
            // services.AddSingleton<SalaryDataService, SalaryDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            services.AddScoped<WeatherStationControllerService>();
            services.AddScoped<WeatherReportControllerService>();
            // services.AddScoped<SalaryControllerService>();
            return services;
        }
    }
}
