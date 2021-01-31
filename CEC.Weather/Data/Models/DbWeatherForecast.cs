/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
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
    public class DbWeatherForecast : IDbRecord<DbWeatherForecast>
    {

        [NotMapped]
        public Guid GUID => Guid.NewGuid();

        [NotMapped]
        public int WeatherForecastID { get => this.ID; }

        [SPParameter(IsID = true, DataType = SqlDbType.Int)]
        public int ID { get; init; } = -1;

        [SPParameter(DataType = SqlDbType.SmallDateTime, ParameterName ="Date")]
        public DateTime Date { get; init; } = DateTime.Now.Date;

        [SPParameter(DataType = SqlDbType.Decimal)]
        public decimal TemperatureC { get; init; } = 20;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string PostCode { get; init; } = string.Empty;

        [SPParameter(DataType = SqlDbType.Bit)]
        public bool Frost { get; init; }

        [SPParameter(DataType = SqlDbType.Int)]
        public int SummaryValue { get; init; }

        [SPParameter(DataType = SqlDbType.Int)]
        public int OutlookValue { get; init; }

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Description { get; init; } = string.Empty;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Detail { get; init; } = string.Empty;

        public string DisplayName { get; init; }

        [NotMapped]
        public decimal TemperatureF => decimal.Round(32 + (TemperatureC / 0.5556M), 2);

        [NotMapped]
        public WeatherSummary Summary => (WeatherSummary)this.SummaryValue;

        [NotMapped]
        public WeatherOutlook Outlook => (WeatherOutlook)this.OutlookValue;

        [NotMapped]
        public DbRecordInfo RecordInfo => DbWeatherForecast.RecInfo;

        public DbWeatherForecast GetFromProperties(RecordCollection recordvalues) => DbWeatherForecast.FromProperties(recordvalues);

        public RecordCollection AsProperties() =>
            new RecordCollection()
            {
                { DataDictionary.__WeatherForecastID, this.ID },
                { DataDictionary.__WeatherForecastDate, this.Date },
                { DataDictionary.__WeatherForecastTemperatureC, this.TemperatureC },
                { DataDictionary.__WeatherForecastTemperatureF, this.TemperatureF },
                { DataDictionary.__WeatherForecastPostCode, this.PostCode },
                { DataDictionary.__WeatherForecastFrost, this.Frost },
                { DataDictionary.__WeatherForecastSummary, this.Summary },
                { DataDictionary.__WeatherForecastSummaryValue, this.SummaryValue },
                { DataDictionary.__WeatherForecastOutlook, this.Outlook },
                { DataDictionary.__WeatherForecastOutlookValue, this.OutlookValue },
                { DataDictionary.__WeatherForecastDescription, this.Description },
                { DataDictionary.__WeatherForecastDetail, this.Detail },
                { DataDictionary.__WeatherForecastDisplayName, this.DisplayName },
        };

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

        public static DbWeatherForecast FromProperties(RecordCollection recordvalues) =>
            new DbWeatherForecast()
            {
                ID = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastID),
                Date = recordvalues.GetEditValue<DateTime>(DataDictionary.__WeatherForecastDate),
                TemperatureC = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherForecastTemperatureC),
                PostCode = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastPostCode),
                Frost = recordvalues.GetEditValue<bool>(DataDictionary.__WeatherForecastFrost),
                SummaryValue = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastSummaryValue),
                OutlookValue = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastOutlookValue),
                Description = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDescription),
                Detail = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDetail),
                DisplayName = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDisplayName),
            };
    }
}
