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

            // Scoped service for the WASM Client version of WeatherForecast Data Service 
            services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
            services.AddScoped<IWeatherStationDataService, WeatherStationWASMDataService>();
            services.AddScoped<IWeatherReportDataService, WeatherReportWASMDataService>();
            services.AddSingleton<SalaryDataService, SalaryDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            services.AddScoped<WeatherStationControllerService>();
            services.AddScoped<WeatherReportControllerService>();
            services.AddScoped<SalaryControllerService>();
            return services;
        }
    }
}
