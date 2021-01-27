using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class WeatherReportControllerService 
        : FactoryControllerService<DbWeatherReport, WeatherForecastDbContext>
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> StationLookupList { get; set; }

        public WeatherReportControllerService(NavigationManager navmanager, IConfiguration appconfiguration,IFactoryDataService<WeatherForecastDbContext> dataService) : base(appconfiguration, navmanager, dataService)
        {
        }

        public async Task LoadLookups()
        {
            this.StationLookupList = await this.GetLookUpListAsync<DbWeatherStation>();
        }
    }
}
