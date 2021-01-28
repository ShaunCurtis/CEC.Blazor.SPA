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
    /// A class to surface Weather Station Edit data
    /// provides Validation and Edit State management
    /// The properties point to the data stored in the underlying RecordCollection
    /// </summary>
    public class WeatherStationEditData : RecordEditData, IRecordEditData
    {
        #region Public

        public string WeatherStationName
        {
            get => this.RecordValues.GetEditValue<string>(DbWeatherStation.__Name);
            set
            {
                this.RecordValues.SetField(DbWeatherStation.__Name, value);
                this.Validate();
            }
        }

        public decimal Latitude
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherStation.__Latitude);
            set
            {
                this.RecordValues.SetField(DbWeatherStation.__Latitude, value);
                this.Validate();
            }
        }

        public decimal Longitude
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherStation.__Longitude);
            set
            {
                this.RecordValues.SetField(DbWeatherStation.__Longitude, value);
                this.Validate();
            }
        }

        public decimal Elevation
        {
            get => this.RecordValues.GetEditValue<decimal>(DbWeatherStation.__Elevation);
            set
            {
                this.RecordValues.SetField(DbWeatherStation.__Elevation, value);
                this.Validate();
            }
        }

        /// <summary>
        /// New Method to load base object
        /// </summary>
        /// <param name="collection"></param>
        public WeatherStationEditData(RecordCollection collection) : base(collection) { }

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
            return this.WeatherStationName.Validation(DbWeatherStation.__Name.FieldName, this, ValidationMessageStore)
                .LongerThan(6, "Name must be longer than 6 letters")
                .Validate();
        }

        private bool ValidateLatitude()
        {
            return this.Latitude.Validation(DbWeatherStation.__Latitude.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-90)
                .LessThanOrEqualTo(90)
                .Validate("Latitude should be in the range -90 to 90");
        }

        private bool ValidateLongitude()
        {
            return this.Longitude.Validation(DbWeatherStation.__Longitude.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-180)
                .LessThanOrEqualTo(180)
                .Validate("Longitude should be in the range -180 to 180");
        }

        private bool ValidateElevation()
        {
            return this.Longitude.Validation(DbWeatherStation.__Elevation.FieldName, this, ValidationMessageStore)
                .GreaterThanOrEqualTo(-1000)
                .LessThanOrEqualTo(10000)
                .Validate("Elevation should be in the range -1000 to 10000");
        }

        #endregion
    }
}
