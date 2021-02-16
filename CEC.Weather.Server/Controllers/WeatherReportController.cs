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
    public class WeatherReportController : ControllerBase
    {
        protected IFactoryDataService<WeatherForecastDbContext> DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherReportController(ILogger<WeatherForecastController> logger, IFactoryDataService<WeatherForecastDbContext> dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [MVC.Route("weatherreport/list")]
        [HttpGet]
        public async Task<List<DbWeatherReport>> GetList() => await DataService.GetRecordListAsync<DbWeatherReport>();

        [MVC.Route("weatherreport/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherReport>> GetFilteredRecordListAsync([FromBody] FilterListCollection filterList) => await DataService.GetFilteredRecordListAsync<DbWeatherReport>(filterList);

        [MVC.Route("weatherreport/lookuplist")]
        [HttpGet]
        public async Task<SortedDictionary<int, string>> GetLookupListAsync() => await DataService.GetLookupListAsync<DbWeatherReport>();

        [MVC.Route("weatherreport/distinctlist")]
        [HttpPost]
        public async Task<List<string>> GetDistinctListAsync([FromBody] string fieldName) => await DataService.GetDistinctListAsync<DbWeatherReport>(fieldName);

        [MVC.Route("weatherreport/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync<DbWeatherReport>();

        [MVC.Route("weatherreport/get")]
        [HttpGet]
        public async Task<DbWeatherReport> GetRec(int id) => await DataService.GetRecordAsync<DbWeatherReport>(id);

        [MVC.Route("weatherreport/read")]
        [HttpPost]
        public async Task<DbWeatherReport> Read([FromBody]int id) => await DataService.GetRecordAsync<DbWeatherReport>(id);

        [MVC.Route("weatherreport/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherReport record) => await DataService.UpdateRecordAsync<DbWeatherReport>(record);

        [MVC.Route("weatherreport/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherReport record) => await DataService.CreateRecordAsync<DbWeatherReport>(record);

        [MVC.Route("weatherreport/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherReport record) => await DataService.DeleteRecordAsync<DbWeatherReport>(record);
    }
}
