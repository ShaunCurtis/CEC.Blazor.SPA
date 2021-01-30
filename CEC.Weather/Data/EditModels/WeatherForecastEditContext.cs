/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using CEC.Blazor.Data;
using CEC.Blazor.Data.Validators;
using CEC.Weather.Data;

namespace CEC.Weather.Data
{

    /// <summary>
    /// A class to surface data stored in the underlying RecordCollection
    /// provides Validation and Edit State management for the Record Collection 
    /// The properties point to the data stored in the underlying RecordCollection
    /// The purposes of the Properties and validation Methods are self-evident 
    /// </summary>
    public class WeatherForecastEditContext : RecordEditContext, IRecordEditContext
    {
        #region Public

        public DateTime WeatherForecastDate
        {
            get => this.RecordValues.GetEditValue<DateTime>(DataDictionary.__WeatherForecastDate);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastDate, value);
                this.Validate();
            }
        }

        public string WeatherForecastPostCode
        {
            get => this.RecordValues.GetEditValue<string>(DataDictionary.__WeatherForecastPostCode);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastPostCode, value);
                this.Validate();
            }
        }

        public string WeatherForecastDescription
        {
            get => this.RecordValues.GetEditValue<string>(DataDictionary.__WeatherForecastDescription);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastDescription, value);
                this.Validate();
            }
        }

        public string WeatherForecastDetail
        {
            get => this.RecordValues.GetEditValue<string>(DataDictionary.__WeatherForecastDetail);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastDetail, value);
                this.Validate();
            }
        }

        public WeatherSummary WeatherForecastSummary
        {
            get => this.RecordValues.GetEditValue<WeatherSummary>(DataDictionary.__WeatherForecastSummary);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastSummary, value);
                this.Validate();
            }
        }

        public WeatherOutlook WeatherForecastOutlook
        {
            get => this.RecordValues.GetEditValue<WeatherOutlook>(DataDictionary.__WeatherForecastOutlook);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastOutlook, value);
                this.Validate();
            }
        }

        public int WeatherForecastOutlookValue
        {
            get => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastOutlookValue);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastOutlookValue, value);
                this.Validate();
            }
        }

        public decimal WeatherForecastTemperatureC
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherForecastTemperatureC);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastTemperatureC, value);
                this.Validate();
            }
        }

        public bool WeatherForecastFrost
        {
            get => this.RecordValues.GetEditValue<bool>(DataDictionary.__WeatherForecastFrost);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherForecastFrost, value);
                this.Validate();
            }
        }

        public int WeatherForecastID
            => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastID);

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
            return this.WeatherForecastDescription.Validation(DataDictionary.__WeatherForecastDescription.FieldName, this, ValidationMessageStore)
                .LongerThan( 6, "Your description needs to be a little longer! 6 letters minimum")
                .Validate();
        }

        private bool ValidatePostCode()
        {
            return this.WeatherForecastPostCode.Validation(DataDictionary.__WeatherForecastPostCode.FieldName, this, ValidationMessageStore)
                .Matches(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$")
                .Validate("You must enter a Post Code (e.g. GL52 8BX)");
        }

        private bool ValidateDate()
        {
            return this.WeatherForecastDate.Validation(DataDictionary.__WeatherForecastDate.FieldName, this, ValidationMessageStore)
                .NotDefault("You must select a date")
                .LessThan(DateTime.Now.AddMonths(1), true, "Date can only be up to 1 month ahead")
                .Validate();
        }

        private bool ValidateTemperatureC()
        {
            return this.WeatherForecastTemperatureC.Validation(DataDictionary.__WeatherForecastTemperatureC.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        #endregion
    }
}
