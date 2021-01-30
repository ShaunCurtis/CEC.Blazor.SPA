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
    /// A class to surface Weather Station Edit data
    /// provides Validation and Edit State management
    /// The properties point to the data stored in the underlying RecordCollection
    /// </summary>
    public class WeatherStationEditContext : RecordEditContext, IRecordEditContext
    {
        #region Public

        public string WeatherStationName
        {
            get => this.RecordValues.GetEditValue<string>(DataDictionary.__WeatherStationName);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherStationName, value);
                this.Validate();
            }
        }

        public decimal WeatherStationLatitude
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherStationLatitude);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherStationLatitude, value);
                this.Validate();
            }
        }

        public decimal WeatherStationLongitude
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherStationLongitude);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherStationLongitude, value);
                this.Validate();
            }
        }

        public decimal WeatherStationElevation
        {
            get => this.RecordValues.GetEditValue<decimal>(DataDictionary.__WeatherStationElevation);
            set
            {
                this.RecordValues.SetField(DataDictionary.__WeatherStationElevation, value);
                this.Validate();
            }
        }

        /// <summary>
        /// New Method to load base object
        /// </summary>
        /// <param name="collection"></param>
        public WeatherStationEditContext(RecordCollection collection) : base(collection) { }

        #endregion

        #region Protected

        protected override void LoadValidationActions()
        {
            base.LoadValidationActions();
            this.ValidationActions.Add(ValidateName);
            this.ValidationActions.Add(ValidateLatitude);
            this.ValidationActions.Add(ValidateLongitude);
            this.ValidationActions.Add(ValidateElevation);
        }

        #endregion

        #region Private

        private bool ValidateName()
        {
            return this.WeatherStationName.Validation(DataDictionary.__WeatherStationName.FieldName, this, ValidationMessageStore)
                .LongerThan(6, "Name must be longer than 6 letters")
                .Validate();
        }

        private bool ValidateLatitude()
        {
            return this.WeatherStationLatitude.Validation(DataDictionary.__WeatherStationLatitude.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-90)
                .LessThanOrEqualTo(90)
                .Validate("Latitude should be in the range -90 to 90");
        }

        private bool ValidateLongitude()
        {
            return this.WeatherStationLongitude.Validation(DataDictionary.__WeatherStationLongitude.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-180)
                .LessThanOrEqualTo(180)
                .Validate("Longitude should be in the range -180 to 180");
        }

        private bool ValidateElevation()
        {
            return this.WeatherStationLongitude.Validation(DataDictionary.__WeatherStationElevation.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-1000)
                .LessThanOrEqualTo(10000)
                .Validate("Elevation should be in the range -1000 to 10000");
        }

        #endregion
    }
}
