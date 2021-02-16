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
    public class WeatherStationController : ControllerBase
    {
        protected IFactoryDataService<WeatherForecastDbContext> DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherStationController(ILogger<WeatherForecastController> logger, IFactoryDataService<WeatherForecastDbContext> dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [MVC.Route("weatherstation/list")]
        [HttpGet]
        public async Task<List<DbWeatherStation>> GetList() => await DataService.GetRecordListAsync<DbWeatherStation>();

        [MVC.Route("weatherstation/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherStation>> GetFilteredRecordListAsync([FromBody] FilterListCollection filterList) => await DataService.GetFilteredRecordListAsync<DbWeatherStation>(filterList);

        [MVC.Route("weatherstation/lookuplist")]
        [HttpGet]
        public async Task<SortedDictionary<int, string>> GetLookupListAsync() => await DataService.GetLookupListAsync<DbWeatherStation>();

        [MVC.Route("weatherstation/distinctlist")]
        [HttpPost]
        public async Task<List<string>> GetDistinctListAsync([FromBody] string fieldName) => await DataService.GetDistinctListAsync<DbWeatherStation>(fieldName);

        [MVC.Route("weatherstation/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync<DbWeatherStation>();

        [MVC.Route("weatherstation/get")]
        [HttpGet]
        public async Task<DbWeatherStation> GetRec(int id) => await DataService.GetRecordAsync<DbWeatherStation>(id);

        [MVC.Route("weatherstation/read")]
        [HttpPost]
        public async Task<DbWeatherStation> Read([FromBody]int id) => await DataService.GetRecordAsync<DbWeatherStation>(id);

        [MVC.Route("weatherstation/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherStation record) => await DataService.UpdateRecordAsync<DbWeatherStation>(record);

        [MVC.Route("weatherstation/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherStation record) => await DataService.CreateRecordAsync<DbWeatherStation>(record);

        [MVC.Route("weatherstation/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherStation record) => await DataService.DeleteRecordAsync<DbWeatherStation>(record);
    }
}
