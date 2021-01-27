using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CEC.Blazor.Data;

namespace CEC.Weather.Services
{
    public class WeatherForecastWASMDataService :
        BaseWASMDataService<DbWeatherForecast, WeatherForecastDbContext>,
        IWeatherForecastDataService
    {
        public WeatherForecastWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
        {
        }
    }
}
