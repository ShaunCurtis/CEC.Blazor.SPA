using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherForecastEditorForm : EditRecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        private WeatherForecastEditContext WeatherForecastEditorContext { get; set; }
       

        protected override Task OnUpdateAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender)
            {
                this.Service = this.ControllerService;
                this.WeatherForecastEditorContext = new WeatherForecastEditContext(this.ControllerService.RecordValueCollection);
                this.RecordEditorContext = this.WeatherForecastEditorContext;
            }
            return base.OnUpdateAsync(firstRender);
        }
    }
}
