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
        public static readonly RecordFieldInfo __ID = new RecordFieldInfo("WeatherReportID");
        public static readonly RecordFieldInfo __Date = new RecordFieldInfo("Date");
        public static readonly RecordFieldInfo __TempMax = new RecordFieldInfo("TempMax");
        public static readonly RecordFieldInfo __TempMin = new RecordFieldInfo("TempMin");
        public static readonly RecordFieldInfo __FrostDays = new RecordFieldInfo("FrostDays");
        public static readonly RecordFieldInfo __Rainfall = new RecordFieldInfo("Rainfall");
        public static readonly RecordFieldInfo __SunHours = new RecordFieldInfo("SunHours");
        public static readonly RecordFieldInfo __WeatherStationID = new RecordFieldInfo("WeatherStationID");
        public static readonly RecordFieldInfo __WeatherStationName = new RecordFieldInfo("WeatherStationName");
        public static readonly RecordFieldInfo __DisplayName = new RecordFieldInfo("DisplayName");
        public static readonly RecordFieldInfo __Month = new RecordFieldInfo("Month");
        public static readonly RecordFieldInfo __Year = new RecordFieldInfo("Year");

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
