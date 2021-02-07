using CEC.Blazor.Services;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.WASM.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Singleton service for the Server Side version of WeatherForecast Data Service 
            // Dummy service produces a new recordset each time the application runs 

            // services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, FactoryServerDataService<WeatherForecastDbContext>>();
             services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, WeatherDummyDataService>();

            // Factory for building the DBContext 
            var dbContext = configuration.GetValue<string>("Configuration:DBContext");
            services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
            return services;
        }
    }
}
