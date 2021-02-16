using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CEC.Weather.Data;
using CEC.Blazor.Data;
using CEC.Blazor.Services;

namespace CEC.Blazor.Server.Controllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        protected IFactoryDataService<WeatherForecastDbContext> DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFactoryDataService<WeatherForecastDbContext> dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [MVC.Route("weatherforecast/list")]
        [HttpGet]
        public async Task<List<DbWeatherForecast>> GetList() => await DataService.GetRecordListAsync<DbWeatherForecast>();

        [MVC.Route("weatherforecast/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherForecast>> GetFilteredRecordListAsync([FromBody] FilterListCollection filterList) => await DataService.GetFilteredRecordListAsync<DbWeatherForecast>(filterList);

        [MVC.Route("weatherforecast/lookuplist")]
        [HttpGet]
        public async Task<SortedDictionary<int, string>> GetLookupListAsync() => await DataService.GetLookupListAsync<DbWeatherForecast>();

        [MVC.Route("weatherforecast/distinctlist")]
        [HttpPost]
        public async Task<List<string>> GetDistinctListAsync([FromBody] string fieldName) => await DataService.GetDistinctListAsync<DbWeatherForecast>(fieldName);

        [MVC.Route("weatherforecast/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync<DbWeatherForecast>();

        [MVC.Route("weatherforecast/get")]
        [HttpGet]
        public async Task<DbWeatherForecast> GetRec(int id) => await DataService.GetRecordAsync<DbWeatherForecast>(id);

        [MVC.Route("weatherforecast/read")]
        [HttpPost]
        public async Task<DbWeatherForecast> Read([FromBody]int id) => await DataService.GetRecordAsync<DbWeatherForecast>(id);

        [MVC.Route("weatherforecast/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherForecast record) => await DataService.UpdateRecordAsync<DbWeatherForecast>(record);

        [MVC.Route("weatherforecast/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherForecast record) => await DataService.CreateRecordAsync<DbWeatherForecast>(record);

        [MVC.Route("weatherforecast/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherForecast record) => await DataService.DeleteRecordAsync<DbWeatherForecast>(record);
    }
}
