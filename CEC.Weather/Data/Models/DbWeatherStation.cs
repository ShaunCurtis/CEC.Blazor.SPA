/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using CEC.Weather.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Foreecast
    /// Data validation is handled by the Fluent Validator
    /// Custom Attributes are for building the EF strored procedures
    /// </summary>
    public class DbWeatherStation 
        :IDbRecord<DbWeatherStation>
    {

        [NotMapped]
        public Guid GUID => Guid.NewGuid();

        [NotMapped]
        public int WeatherStationID { get => this.ID; }

        [SPParameter(IsID = true, DataType = SqlDbType.Int)]
        public int ID { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Name { get; set; } = "No Name";

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName ="decimal(8,4)")]
        public decimal Latitude { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal Longitude { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Elevation { get; set; } = 1000;

        public string DisplayName { get; set; }

        [NotMapped]
        public string LatLong => $"{this.Latitude.AsLatitude()} {this.Longitude.AsLongitude()}";

        [NotMapped]
        public DbRecordInfo RecordInfo => DbWeatherStation.RecInfo;

        [NotMapped]
        public static DbRecordInfo RecInfo => new DbRecordInfo()
        {
            CreateSP = "sp_Create_WeatherStation",
            UpdateSP = "sp_Update_WeatherStation",
            DeleteSP = "sp_Delete_WeatherStation",
            RecordDescription = "Weather Station",
            RecordName = "WeatherStation",
            RecordListDescription = "Weather Stations",
            RecordListName = "WeatherStations"
        };

        public RecordCollection AsProperties() =>
            new RecordCollection()
            {
                { DataDictionary.__WeatherStationID, this.ID },
                { DataDictionary.__WeatherStationName, this.Name },
                { DataDictionary.__WeatherStationLatitude, this.Latitude },
                { DataDictionary.__WeatherStationLongitude, this.Longitude },
                { DataDictionary.__WeatherStationElevation, this.Elevation }
        };

        public static DbWeatherStation FromProperties(RecordCollection recordvalues) =>
            new DbWeatherStation()
            {
                ID = recordvalues.GetEditValue<int>(DataDictionary.__WeatherStationID),
                Name = recordvalues.GetEditValue<string>(DataDictionary.__WeatherStationName),
                Latitude = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherStationLatitude),
                Longitude = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherStationLongitude),
                Elevation = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherStationElevation)
            };

        public DbWeatherStation GetFromProperties(RecordCollection recordvalues) => DbWeatherStation.FromProperties(recordvalues);

    }
}
