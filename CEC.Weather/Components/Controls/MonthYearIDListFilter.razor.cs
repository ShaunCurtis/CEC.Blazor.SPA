﻿/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CEC.Blazor.Core;
using CEC.Blazor.SPA.Components;

namespace CEC.Weather.Components
{
    public partial class MonthYearIDListFilter : BaseBlazorComponent
    {
        [Inject]
        private WeatherReportControllerService Service { get; set; }

        [Parameter]
        public bool ShowID { get; set; } = true;

        private SortedDictionary<int, string> MonthLookupList { get; set; }

        private SortedDictionary<int, string> YearLookupList { get; set; }

        private SortedDictionary<int, string> IdLookupList { get; set; }

        private EditContext EditContext => new EditContext(this.Service.Record);

        private int OldMonth = 0;
        private int OldYear = 0;
        private long OldID = 0;

        private int Month
        {
            get => this.Service.FilterList.TryGetFilter("Month", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Month", value);
                else this.Service.FilterList.DeleteFilter("Month");
                if (this.Month != this.OldMonth)
                {
                    this.OldMonth = this.Month;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        private int Year
        {
            get => this.Service.FilterList.TryGetFilter("Year", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Year", value);
                else this.Service.FilterList.DeleteFilter("Year");
                if (this.Year != this.OldYear)
                {
                    this.OldYear = this.Year;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        private int ID
        {
            get => this.Service.FilterList.TryGetFilter("WeatherStationID", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("WeatherStationID", value);
                else this.Service.FilterList.DeleteFilter("WeatherStationID");
                if (this.ID != this.OldID)
                {
                    this.OldID = this.ID;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        protected async override Task OnUpdateAsync(bool firstRender)
        {
            this.OldYear = this.Year;
            this.OldMonth = this.Month;
            this.Service.FilterList.OnlyLoadIfFilters = true;
            await GetLookupsAsync();
        }

        protected async Task GetLookupsAsync()
        {
            this.IdLookupList = await this.Service.GetLookUpListAsync<DbWeatherStation>("-- ALL STATIONS --");
            this.MonthLookupList = new SortedDictionary<int, string> { { 0, "-- ALL MONTHS --" } };
            for (int i = 1; i < 13; i++) 
                this.MonthLookupList.Add(i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
            var yearLookupList = await this.Service.GetDistinctListAsync<DbWeatherReport>("Year");
            yearLookupList ??= new List<string>();
            this.YearLookupList = new SortedDictionary<int, string> { { 0, "-- ALL YEARS --" } };
            yearLookupList.ForEach(item => this.YearLookupList.Add(int.Parse(item), item));

        }
    }
}
