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
    public class WeatherReportEditData : RecordEditData, IRecordEditData
    {
        #region Public

        public DateTime Date
        {
            get => this.RecordValues.GetEditValue<DateTime>(DbWeatherReport.__Date);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__Date, value);
                this.Validate();
            }
        }

        public decimal TempMax
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherReport.__TempMax);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__TempMax, value);
                this.Validate();
            }
        }

        public decimal TempMin
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherReport.__TempMin);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__TempMin, value);
                this.Validate();
            }
        }

        public int FrostDays
        {
            get => this.RecordValues.GetEditValue<int>(DbWeatherReport.__FrostDays);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__FrostDays, value);
                this.Validate();
            }
        }


        public decimal Rainfall
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherReport.__Rainfall);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__Rainfall, value);
                this.Validate();
            }
        }


        public decimal SunHours
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherReport.__SunHours);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__SunHours, value);
                this.Validate();
            }
        }


        public int WeatherStationID
        {
            get => this.RecordValues.GetEditValue<int>(DbWeatherReport.__WeatherStationID);
            set
            {
                this.RecordValues.SetField(DbWeatherReport.__WeatherStationID, value);
                this.Validate();
            }
        }

        public bool WeatherReportID
        {
            get => this.RecordValues.GetEditValue<bool>(DbWeatherForecast.__ID);
        }

        /// <summary>
        /// New Method to load base object
        /// </summary>
        /// <param name="collection"></param>
        public WeatherReportEditData(RecordCollection collection) : base(collection) { }

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
            return this.Date.Validation(DbWeatherReport.__Date.FieldName, this, ValidationMessageStore)
                .NotDefault("You must select a date")
                .Validate();
        }

        private bool ValidateTempMax()
        {
            return this.TempMax.Validation(DbWeatherReport.__TempMax.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        private bool ValidateTempMin()
        {
            return this.TempMin.Validation(DbWeatherReport.__TempMax.FieldName, this, ValidationMessageStore)
                .LessThan(70, "The temperature must be less than 70C")
                .GreaterThan(-60, "The temperature must be greater than -60C")
                .Validate();
        }

        private bool ValidateFrostDays()
        {
            return this.TempMin.Validation(DbWeatherReport.__FrostDays.FieldName, this, ValidationMessageStore)
                .LessThan(32)
                .GreaterThan(-1)
                .Validate("There are between 0 and 31 frost days in a month");
        }


        private bool ValidateRainfall()
        {
            return this.Rainfall.Validation(DbWeatherReport.__Rainfall.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(0, "Rainfall can't be a negative amount")
                .Validate();
        }

        private bool ValidateSunHours()
        {
            return this.SunHours.Validation(DbWeatherReport.__SunHours.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(0, "Sun hours per month can't be a negative amount")
                .Validate();
        }


        #endregion
    }
}
