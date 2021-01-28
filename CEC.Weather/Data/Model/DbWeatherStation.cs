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
        public static readonly RecordFieldInfo __ID = new RecordFieldInfo("WeatherStationID");
        public static readonly RecordFieldInfo __Name = new RecordFieldInfo("Name");
        public static readonly RecordFieldInfo __Latitude = new RecordFieldInfo("Latitude");
        public static readonly RecordFieldInfo __Longitude = new RecordFieldInfo("Longitude");
        public static readonly RecordFieldInfo __Elevation = new RecordFieldInfo("Elevation");

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
                { __ID, this.ID },
                { __Name, this.Name },
                { __Latitude, this.Latitude },
                { __Longitude, this.Longitude },
                { __Elevation, this.Elevation }
        };

        public static DbWeatherStation FromProperties(RecordCollection recordvalues) =>
            new DbWeatherStation()
            {
                ID = recordvalues.GetEditValue<int>(__ID),
                Name = recordvalues.GetEditValue<string>(__Name),
                Latitude = recordvalues.GetEditValue<decimal>(__Latitude),
                Longitude = recordvalues.GetEditValue<decimal>(__Longitude),
                Elevation = recordvalues.GetEditValue<decimal>(__Elevation)
            };

        public DbWeatherStation GetFromProperties(RecordCollection recordvalues) => DbWeatherStation.FromProperties(recordvalues);

    }
}
