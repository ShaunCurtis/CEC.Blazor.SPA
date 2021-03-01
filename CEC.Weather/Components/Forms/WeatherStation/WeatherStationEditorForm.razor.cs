/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.SPA.Components.Forms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationEditorForm : EditRecordFormBase<DbWeatherStation, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherStationControllerService ControllerService { get; set; }

        private WeatherStationEditContext EditorContext { get; set; }


        protected override Task OnUpdateAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender)
            {
                this.Service = this.ControllerService;
                this.EditorContext = new WeatherStationEditContext(this.ControllerService.RecordValueCollection);
                this.RecordEditorContext = this.EditorContext;
            }
            return base.OnUpdateAsync(firstRender);
        }
    }
}
