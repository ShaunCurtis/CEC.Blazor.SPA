using CEC.Blazor.SPA.Components;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CEC.Weather.Services
{
    public class WeatherForecastControllerService : 
        FactoryControllerService<DbWeatherForecast, WeatherForecastDbContext> 
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IFactoryDataService<WeatherForecastDbContext> dataService) : base(appconfiguration, navmanager,dataService)
        {
        }
    }
}
