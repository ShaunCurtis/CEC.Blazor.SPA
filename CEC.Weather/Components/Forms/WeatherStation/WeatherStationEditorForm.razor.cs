﻿/// =================================
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

        private WeatherStationEditContext WeatherStationEditorContext { get; set; }


        protected override Task OnRenderAsync(bool firstRender)
        {
            // Assign the correct controller service
            if (firstRender)
            {
                this.Service = this.ControllerService;
                this.WeatherStationEditorContext = new WeatherStationEditContext(this.ControllerService.RecordValueCollection);
                this.RecordEditorContext = this.WeatherStationEditorContext;
            }
            return base.OnRenderAsync(firstRender);
        }
    }
}
