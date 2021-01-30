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
        public DbRecordInfo RecordInfo => DbWeatherReport.RecInfo;

        [NotMapped]
        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);

        [NotMapped]
        public string MonthYearName => $"{this.MonthName}-{this.Year}";


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
                { DataDictionary.__WeatherReportID, this.ID },
                { DataDictionary.__WeatherReportDate, this.Date },
                { DataDictionary.__WeatherReportTempMax, this.TempMax },
                { DataDictionary.__WeatherReportTempMin, this.TempMin },
                { DataDictionary.__WeatherReportFrostDays, this.FrostDays },
                { DataDictionary.__WeatherReportRainfall, this.Rainfall },
                { DataDictionary.__WeatherReportSunHours, this.SunHours },
                { DataDictionary.__WeatherReportDisplayName, this.DisplayName },
                { DataDictionary.__WeatherStationID, this.WeatherStationID },
                { DataDictionary.__WeatherStationName, this.WeatherStationName },
                { DataDictionary.__WeatherReportMonth, this.Month },
                { DataDictionary.__WeatherReportYear, this.Year }
        };

        public static DbWeatherReport FromProperties(RecordCollection recordvalues) =>
            new DbWeatherReport()
            {
                ID = recordvalues.GetEditValue<int>(DataDictionary.__WeatherReportID),
                Date = recordvalues.GetEditValue<DateTime>(DataDictionary.__WeatherReportDate),
                TempMax = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherReportTempMax),
                TempMin = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherReportTempMin),
                FrostDays = recordvalues.GetEditValue<int>(DataDictionary.__WeatherReportFrostDays),
                Rainfall = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherReportRainfall),
                SunHours = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherReportSunHours),
                DisplayName = recordvalues.GetEditValue<string>(DataDictionary.__WeatherReportDisplayName),
                WeatherStationID = recordvalues.GetEditValue<int>(DataDictionary.__WeatherStationID),
                WeatherStationName = recordvalues.GetEditValue<string>(DataDictionary.__WeatherStationName),
                Month = recordvalues.GetEditValue<int>(DataDictionary.__WeatherReportMonth),
                Year = recordvalues.GetEditValue<int>(DataDictionary.__WeatherReportYear),
            };

        public DbWeatherReport GetFromProperties(RecordCollection recordvalues) => DbWeatherReport.FromProperties(recordvalues);

    }
}
