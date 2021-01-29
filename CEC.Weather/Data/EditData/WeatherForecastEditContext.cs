/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using CEC.Blazor.Data;
using CEC.Blazor.Data.Validators;
using CEC.Weather.Data;

namespace CEC.Workflow.Components
{

    /// <summary>
    /// A class to surface data stored in the underlying RecordCollection
    /// provides Validation and Edit State management for the Record Collection 
    /// The properties point to the data stored in the underlying RecordCollection
    /// </summary>
    public class WeatherForecastEditContext : RecordEditContext, IRecordEditContext
    {
        #region Public

        public DateTime Date
        {
            get => this.RecordValues.GetEditValue<DateTime>(DbWeatherForecast.__Date);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Date, value);
                this.Validate();
            }
        }

        public string PostCode
        {
            get => this.RecordValues.GetEditValue<string>(DbWeatherForecast.__PostCode);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__PostCode, value);
                this.Validate();
            }
        }

        public string Description
        {
            get => this.RecordValues.GetEditValue<string>(DbWeatherForecast.__Description);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Description, value);
                this.Validate();
            }
        }

        public string Detail
        {
            get => this.RecordValues.GetEditValue<string>(DbWeatherForecast.__Detail);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Detail, value);
                this.Validate();
            }
        }

        public WeatherSummary Summary
        {
            get => this.RecordValues.GetEditValue<WeatherSummary>(DbWeatherForecast.__Summary);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Summary, value);
                this.Validate();
            }
        }

        public WeatherOutlook Outlook
        {
            get => this.RecordValues.GetEditValue<WeatherOutlook>(DbWeatherForecast.__Outlook);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Outlook, value);
                this.Validate();
            }
        }

        public int OutlookValue
        {
            get => this.RecordValues.GetEditValue<int>(DbWeatherForecast.__OutlookValue);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__OutlookValue, value);
                this.Validate();
            }
        }

        public decimal TemperatureC
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherForecast.__TemperatureC);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__TemperatureC, value);
                this.Validate();
            }
        }

        public bool Frost
        {
            get => this.RecordValues.GetEditValue<bool>(DbWeatherForecast.__Frost);
            set
            {
                this.RecordValues.SetField(DbWeatherForecast.__Detail, value);
                this.Validate();
            }
        }

        public bool WeatherForecastID
        {
            get => this.RecordValues.GetEditValue<bool>(DbWeatherForecast.__ID);
        }

        /// <summary>
        /// New Method to load base object
        /// </summary>
        /// <param name="collection"></param>
        public WeatherForecastEditContext(RecordCollection collection) : base(collection) { }

        #endregion

        #region Protected

        protected override void LoadValidationActions()
        {
            this.ValidationActions.Add(ValidateDescription);
            this.ValidationActions.Add(ValidateTemperatureC);
            this.ValidationActions.Add(ValidateDate);
            this.ValidationActions.Add(ValidatePostCode);
        }

        #endregion

        #region Private

        private bool ValidateDescription()
        {
            return this.Description.Validation(DbWeatherForecast.__Description.FieldName, this, ValidationMessageStore)
                .LongerThan( 6, "Your description needs to be a little longer! 6 letters minimum")
                .Validate();
        }

        private bool ValidatePostCode()
        {
            return this.PostCode.Validation(DbWeatherForecast.__PostCode.FieldName, this, ValidationMessageStore)
                .Matches(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$")
                .Validate("You must enter a Post Code (e.g. GL52 8BX)");
        }

        private bool ValidateDate()
        {
            return this.Date.Validation(DbWeatherForecast.__Date.FieldName, this, ValidationMessageStore)
                .NotDefault("You must select a date")
                .LessThan(DateTime.Now.AddMonths(1), true, "Date can only be up to 1 month ahead")
                .Validate();
        }

        private bool ValidateTemperatureC()
        {
            return this.TemperatureC.Validation(DbWeatherForecast.__TemperatureC.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        #endregion
    }
}
