using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherReportEditorForm : EditRecordFormBase<DbWeatherReport, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherReportControllerService ControllerService { get; set; }

        public SortedDictionary<int, string> StationLookupList { get; set; }

        private WeatherReportEditContext EditorContext { get; set; }

        protected async override Task OnRenderAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender)
            {
                this.Service = this.ControllerService;
                this.EditorContext = new WeatherReportEditContext(this.ControllerService.RecordValueCollection);
                this.RecordEditorContext = this.EditorContext;
            }
            StationLookupList = await this.Service.GetLookUpListAsync<DbWeatherStation>();
            await base.OnRenderAsync(firstRender);
        }
    }
}
