using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using CEC.Workflow.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherForecastEditorForm : EditRecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        private WeatherForecastEditData EditData { get; set; }
        
        protected override Task OnRenderAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender)
            {
                this.Service = this.ControllerService;
                this.EditData = new WeatherForecastEditData(this.ControllerService.RecordValueCollection);
                this.RecordEditData = this.EditData;
            }
            return base.OnRenderAsync(firstRender);
        }
    }
}
