/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using CEC.Blazor.Data;
using CEC.Blazor.Data.Validators;

namespace CEC.Weather.Data
{

    /// <summary>
    /// A class to surface data stored in the underlying RecordCollection
    /// provides Validation and Edit State management for the Record Collection 
    /// The properties point to the data stored in the underlying RecordCollection
    /// </summary>
    public class WeatherReportEditContext : RecordEditContext, IRecordEditContext
    {
        #region Public

        public DateTime WeatherReportDate
        {
            get => this.RecordValues.GetEditValue<DateTime>(DataDictionary.__WeatherReportDate);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportDate, value);
                this.Validate();
            }
        }

        public decimal WeatherReportTempMax
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherReportTempMax);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportTempMax, value);
                this.Validate();
            }
        }

        public decimal WeatherReportTempMin
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherReportTempMin);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportTempMin, value);
                this.Validate();
            }
        }

        public int WeatherReportFrostDays
        {
            get => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherReportFrostDays);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportFrostDays, value);
                this.Validate();
            }
        }


        public decimal WeatherReportRainfall
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherReportRainfall);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportRainfall, value);
                this.Validate();
            }
        }


        public decimal WeatherReportSunHours
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherReportSunHours);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherReportSunHours, value);
                this.Validate();
            }
        }


        public int WeatherStationID
        {
            get => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherStationID);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherStationID, value);
                this.Validate();
            }
        }

        public bool WeatherReportID
        {
            get => this.RecordValues.GetEditValue<bool>(DataDictionary.__WeatherReportID);
        }

        /// <summary>
        /// New Method to load base object
        /// </summary>
        /// <param name="collection"></param>
        public WeatherReportEditContext(RecordCollection collection) : base(collection) { }

        #endregion

        #region Protected

        protected override void LoadValidationActions()
        {
            base.LoadValidationActions();
            this.ValidationActions.Add(ValidateTempMax);
            this.ValidationActions.Add(ValidateTempMin);
            this.ValidationActions.Add(ValidateDate);
            this.ValidationActions.Add(ValidateFrostDays);
            this.ValidationActions.Add(ValidateRainfall);
            this.ValidationActions.Add(ValidateSunHours);
        }

        #endregion

        #region Private

        private bool ValidateDate()
        {
            return this.WeatherReportDate.Validation(DataDictionary.__WeatherReportDate.FieldName, this, ValidationMessageStore)
                .NotDefault("You must select a date")
                .Validate();
        }

        private bool ValidateTempMax()
        {
            return this.WeatherReportTempMax.Validation(DataDictionary.__WeatherReportTempMax.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        private bool ValidateTempMin()
        {
            return this.WeatherReportTempMin.Validation(DataDictionary.__WeatherReportTempMin.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        private bool ValidateFrostDays()
        {
            return this.WeatherReportTempMin.Validation(DataDictionary.__WeatherReportFrostDays.FieldName, this, ValidationMessageStore)
                .LessThan(32)
                .GreaterThan(-1)
                .Validate("There are between 0 and 31 frost days in a month");
        }


        private bool ValidateRainfall()
        {
            return this.WeatherReportRainfall.Validation(DataDictionary.__WeatherReportRainfall.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(0, "Rainfall can't be a negative amount")
                .Validate();
        }

        private bool ValidateSunHours()
        {
            return this.WeatherReportSunHours.Validation(DataDictionary.__WeatherReportSunHours.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(0, "Sun hours per month can't be a negative amount")
                .Validate();
        }


        #endregion
    }
}
