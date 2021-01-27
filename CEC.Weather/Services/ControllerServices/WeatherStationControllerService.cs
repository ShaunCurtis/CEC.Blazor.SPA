using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CEC.Weather.Services
{
    public class WeatherStationControllerService : 
        FactoryControllerService<DbWeatherStation, WeatherForecastDbContext>
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        public WeatherStationControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IFactoryDataService<WeatherForecastDbContext> dataService) : base(appconfiguration, navmanager, dataService)
        {
        }
    }
}
