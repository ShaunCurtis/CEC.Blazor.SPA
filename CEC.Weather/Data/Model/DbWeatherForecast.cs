using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
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
    public class DbWeatherForecast :IDbRecord<DbWeatherForecast>
    {
        public static readonly RecordFieldInfo __ID = new RecordFieldInfo("WeatherForecastID");
        public static readonly RecordFieldInfo __Date = new RecordFieldInfo("WeatherForecastDate");
        public static readonly RecordFieldInfo __TemperatureC = new RecordFieldInfo("TemperatureC");
        public static readonly RecordFieldInfo __TemperatureF = new RecordFieldInfo("TemperatureF");
        public static readonly RecordFieldInfo __PostCode = new RecordFieldInfo("PostCode");
        public static readonly RecordFieldInfo __Frost = new RecordFieldInfo("Frost");
        public static readonly RecordFieldInfo __Summary = new RecordFieldInfo("Summary");
        public static readonly RecordFieldInfo __SummaryValue = new RecordFieldInfo("SummaryValue");
        public static readonly RecordFieldInfo __Outlook = new RecordFieldInfo("Outlook");
        public static readonly RecordFieldInfo __OutlookValue = new RecordFieldInfo("OutlookValue");
        public static readonly RecordFieldInfo __Description = new RecordFieldInfo("Description");
        public static readonly RecordFieldInfo __Detail = new RecordFieldInfo("Detail");
        public static readonly RecordFieldInfo __DisplayName = new RecordFieldInfo("DisplayName");

        [NotMapped]
        public Guid GUID => Guid.NewGuid();

        [NotMapped]
        public int WeatherForecastID { get => this.ID; }

        [SPParameter(IsID = true, DataType = SqlDbType.Int)]
        public int ID { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.SmallDateTime)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [SPParameter(DataType = SqlDbType.Decimal)]
        public decimal TemperatureC { get; set; } = 20;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string PostCode { get; set; } = string.Empty;

        [SPParameter(DataType = SqlDbType.Bit)]
        public bool Frost { get; set; }

        [SPParameter(DataType = SqlDbType.Int)]
        public int SummaryValue
        {
            get => (int)this.Summary;
            set => this.Summary = (WeatherSummary)value;
        }

        [SPParameter(DataType = SqlDbType.Int)]
        public int OutlookValue
        {
            get => (int)this.Outlook;
            set => this.Outlook = (WeatherOutlook)value;
        }

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Description { get; set; } = string.Empty;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Detail { get; set; } = string.Empty;

        public string DisplayName { get; set; }

        [NotMapped]
        public decimal TemperatureF => decimal.Round(32 + (TemperatureC / 0.5556M), 2);

        [NotMapped]
        public WeatherSummary Summary { get; set; } = WeatherSummary.Unknown;

        [NotMapped]
        public WeatherOutlook Outlook { get; set; } = WeatherOutlook.Sunny;

        [NotMapped]
        public DbRecordInfo RecordInfo => DbWeatherForecast.RecInfo;

        [NotMapped]
        public static DbRecordInfo RecInfo => new DbRecordInfo()
        {
            CreateSP = "sp_Create_WeatherForecast",
            UpdateSP = "sp_Update_WeatherForecast",
            DeleteSP = "sp_Delete_WeatherForecast",
            RecordDescription = "Weather Forecast",
            RecordName = "WeatherForecast",
            RecordListDescription = "Weather Forecasts",
            RecordListName = "WeatherForecasts"
        };

        public RecordCollection AsProperties() =>
            new RecordCollection()
            {
                { __ID, this.ID },
                { __Date, this.Date },
                { __TemperatureC, this.TemperatureC },
                { __TemperatureF, this.TemperatureF },
                { __PostCode, this.PostCode },
                { __Frost, this.Frost },
                { __Summary, this.Summary },
                { __SummaryValue, this.SummaryValue },
                { __Outlook, this.Outlook },
                { __OutlookValue, this.OutlookValue },
                { __Description, this.Description },
                { __Detail, this.Detail },
                { __DisplayName, this.DisplayName },
        };


        public static DbWeatherForecast FromProperties(RecordCollection recordvalues) =>
            new DbWeatherForecast()
            {
                ID = recordvalues.GetEditValue<int>(__ID),
                Date = recordvalues.GetEditValue<DateTime>(__Date),
                TemperatureC = recordvalues.GetEditValue<decimal>(__TemperatureC),
                PostCode = recordvalues.GetEditValue<string>(__PostCode),
                Frost = recordvalues.GetEditValue<bool>(__Frost),
                Summary = recordvalues.GetEditValue<WeatherSummary>(__Summary),
                SummaryValue = recordvalues.GetEditValue<int>(__SummaryValue),
                Outlook = recordvalues.GetEditValue<WeatherOutlook>(__Outlook),
                OutlookValue = recordvalues.GetEditValue<int>(__OutlookValue),
                Description = recordvalues.GetEditValue<string>(__Description),
                Detail = recordvalues.GetEditValue<string>(__Detail),
                DisplayName = recordvalues.GetEditValue<string>(__DisplayName),

            };

        public DbWeatherForecast GetFromProperties(RecordCollection recordvalues) => DbWeatherForecast.FromProperties(recordvalues);

    }
}
