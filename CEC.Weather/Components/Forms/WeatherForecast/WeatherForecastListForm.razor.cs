using Microsoft.AspNetCore.Components;
using CEC.Blazor.SPA.Components.Forms;
using CEC.Blazor.Extensions;
using CEC.Weather.Data;
using CEC.Weather.Services;
using System.Threading.Tasks;
using CEC.Weather.Components.Views;

namespace CEC.Weather.Components
{
    public partial class WeatherForecastListForm : ListFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        /// <summary>
        /// The Injected Controller service for this record
        /// </summary>
        [Inject] protected WeatherForecastControllerService ControllerService { get; set; }

        private bool UseModal => this.Properties?.Get<bool>(PropertyConstants.UseModalViewer) ?? false;

        protected override Task OnUpdateAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Sets the specific service
                this.Service = this.ControllerService;
                // Sets the max column
                this.Properties.Set(PropertyConstants.MaxColumn, 3);
            }
            return base.OnUpdateAsync(firstRender);
        }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected void OnView(int id)
        {
            if (this.UseModal && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastViewerForm>(id);
            else this.OnViewAsync<WeatherForecastViewerView>(id);
        }

        /// <summary>
        /// Method called when the user clicks on a row Edit button.
        /// </summary>
        /// <param name="id"></param>
        protected void OnEdit(int id)
        {
            if (this.UseModal && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastEditorForm>(id);
            else this.OnViewAsync<WeatherForecastEditorView>(id);
        }
    }
}
