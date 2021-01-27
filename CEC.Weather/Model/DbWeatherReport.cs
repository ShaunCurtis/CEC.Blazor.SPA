using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Foreecast
    /// Data validation is handled by the Fluent Validator
    /// Custom Attributes are for building the EF strored procedures
    /// </summary>
    public class DbWeatherReport :IDbRecord<DbWeatherReport>
    {
        public static readonly string __ID = "WeatherReportID";
        public static readonly string __Date = "Date";
        public static readonly string __TempMax = "TempMax";
        public static readonly string __TempMin = "TempMin";
        public static readonly string __FrostDays = "FrostDays";
        public static readonly string __Rainfall = "Rainfall";
        public static readonly string __SunHours = "SunHours";
        public static readonly string __WeatherStationID = "WeatherStationID";
        public static readonly string __WeatherStationName = "WeatherStationName";
        public static readonly string __DisplayName = "DisplayName";
        public static readonly string __Month = "Month";
        public static readonly string __Year = "Year";

        [NotMapped]
        public Guid GUID => Guid.NewGuid();

        [NotMapped]
        public int WeatherReportID { get => this.ID; }

        [SPParameter(IsID = true, DataType = SqlDbType.Int)]
        public int ID { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.Int)]
        public int WeatherStationID { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.SmallDateTime)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal TempMax { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal TempMin { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Int)]
        public int FrostDays { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal Rainfall { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal SunHours { get; set; } = -1;

        public string DisplayName { get; set; }

        public string WeatherStationName { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [NotMapped]
        public DbRecordInfo RecordInfo => DbWeatherForecast.RecInfo;

        [NotMapped]
        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);

        [NotMapped]
        public string MonthYearName => $"{this.MonthName}-{this.Year}";

        public void SetNew() => this.ID = 0;

        public DbWeatherReport ShadowCopy()
        {
            return new DbWeatherReport() {
                ID = this.ID,
                Date = this.Date,
                TempMax = this.TempMax,
                TempMin = this.TempMin,
                FrostDays = this.FrostDays,
                Rainfall = this.Rainfall,
                SunHours = this.SunHours,
                DisplayName = this.DisplayName,
                WeatherStationID = this.WeatherStationID,
                WeatherStationName = this.WeatherStationName
            };
        }

        [NotMapped]
        public static DbRecordInfo RecInfo => new DbRecordInfo()
        {
            CreateSP = "sp_Create_WeatherReport",
            UpdateSP = "sp_Update_WeatherReport",
            DeleteSP = "sp_Delete_WeatherReport",
            RecordDescription = "Weather Report",
            RecordName = "WeatherReport",
            RecordListDescription = "Weather Reports",
            RecordListName = "WeatherReports"
        };

        public RecordCollection AsProperties() =>
            new RecordCollection()
            {
                { __ID, this.ID },
                { __Date, this.Date },
                { __TempMax, this.TempMax },
                { __TempMin, this.TempMin },
                { __FrostDays, this.FrostDays },
                { __Rainfall, this.Rainfall },
                { __SunHours, this.SunHours },
                { __DisplayName, this.DisplayName },
                { __WeatherStationID, this.WeatherStationID },
                { __WeatherStationName, this.WeatherStationName },
                { __Month, this.Month },
                { __Year, this.Year }
        };

        public static DbWeatherReport FromProperties(RecordCollection recordvalues) =>
            new DbWeatherReport()
            {
                ID = recordvalues.GetEditValue<int>(__ID),
                Date = recordvalues.GetEditValue<DateTime>(__Date),
                TempMax = recordvalues.GetEditValue<decimal>(__TempMax),
                TempMin = recordvalues.GetEditValue<decimal>(__TempMin),
                FrostDays = recordvalues.GetEditValue<int>(__FrostDays),
                Rainfall = recordvalues.GetEditValue<decimal>(__Rainfall),
                SunHours = recordvalues.GetEditValue<decimal>(__SunHours),
                DisplayName = recordvalues.GetEditValue<string>(__DisplayName),
                WeatherStationID = recordvalues.GetEditValue<int>(__WeatherStationID),
                WeatherStationName = recordvalues.GetEditValue<string>(__WeatherStationName),
                Month = recordvalues.GetEditValue<int>(__Month),
                Year = recordvalues.GetEditValue<int>(__Year),
            };

        public DbWeatherReport GetFromProperties(RecordCollection recordvalues) => DbWeatherReport.FromProperties(recordvalues);

    }
}
