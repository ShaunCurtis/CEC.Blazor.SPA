using CEC.Blazor.SPA.Components;
using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CEC.Weather.Services
{
    public class WeatherDummyDataService :
        FactoryDataService<WeatherForecastDbContext>
    {

        /// <summary>
        /// internal Property to hold the dummy records for CRUD operations
        /// </summary>
        private List<DbWeatherForecast> WeatherForecastRecords { get; set; }

        private List<DbWeatherStation> WeatherStationRecords { get; set; }

        private List<DbWeatherReport> WeatherReportRecords { get; set; }

        public WeatherDummyDataService(IConfiguration configuration): base(configuration)
        {
            this.AppConfiguration = configuration;
            this.GetWeatherForecastRecords(100);
            this.GetWeatherStationRecords();
            this.GetWeatherReportRecords();
        }

        /// <summary>
        /// Method to get a set of 100 dummy records
        /// </summary>
        /// <param name="recordcount"></param>
        private void GetWeatherForecastRecords(int recordcount)
        {
            this.WeatherForecastRecords = new List<DbWeatherForecast>();
            for (var i = 1; i <= recordcount; i++)
            {
                var rng = new Random();
                var temperatureC = rng.Next(-5, 35);
                var date = DateTime.Now.AddDays(-(recordcount - i));
                var summaryValue = rng.Next(11);
                var outlookValue = rng.Next(3);
                var rec = new DbWeatherForecast()
                {
                    ID = i,
                    Date = date,
                    TemperatureC = temperatureC,
                    SummaryValue = summaryValue,
                    OutlookValue = outlookValue,
                    Frost = temperatureC < 0,
                    PostCode = "GL2 5TP",
                    Description = $"The Weather forecast for {date.DayOfWeek} {date.ToLongDateString()} is mostly {(WeatherOutlook)outlookValue} and {(WeatherSummary)summaryValue}"
                };
                WeatherForecastRecords.Add(rec);
            }
        }

        private void GetWeatherStationRecords()
        {
            this.WeatherStationRecords = new List<DbWeatherStation>();
            var rec = new DbWeatherStation()
            {
                ID = 1,
                Name = "Tiree",
                Longitude = -1.5m,
                Latitude = 54.2m,
                DisplayName = "Tiree",
                Elevation = 28

            };
            this.WeatherStationRecords.Add(rec);
            rec = new DbWeatherStation()
            {
                ID = 2,
                Name = "Ross-on-Wye",
                Longitude = -1.2m,
                Latitude = 52.2m,
                DisplayName = "Ross-on-Wye",
                Elevation = 120

            };
            this.WeatherStationRecords.Add(rec);
        }

        private void GetWeatherReportRecords()
        {
            this.WeatherReportRecords = new List<DbWeatherReport>();
            var i = 1;
            for (var id = 1; id <= 2; id++)
            {
                var stationname = "Tiree";
                var date = new DateTime(1970, 1, 1);
                if (id == 2) stationname = "Ross-on-Wye";
                while (date < DateTime.Now)
                {
                    var rng = new Random();
                    var tempmin = rng.Next(-5, 18);
                    var tempmax = tempmin + rng.Next(0, 18);
                    var rec = new DbWeatherReport()
                    {
                        ID = i++,
                        WeatherStationID = id,
                        WeatherStationName = stationname,
                        Date = date,
                        TempMin = tempmin,
                        TempMax = tempmax,
                        FrostDays = rng.Next(0, 20),
                        Rainfall = rng.Next(0, 200),
                        SunHours = rng.Next(0, 200),
                        Month = date.Month,
                        Year = date.Year,
                        DisplayName = $"Record for {date.Month}-{date.Year}"
                    };
                    this.WeatherReportRecords.Add(rec);
                    date = date.AddMonths(1);
                }
            }
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<List<TRecord>> GetRecordListAsync<TRecord>()
        {
            var list = new List<TRecord>();

            var recordname = this.GetRecordType<TRecord>();
            switch (this.GetRecordType<TRecord>())
            {
                case "WeatherForecast":
                    this.WeatherForecastRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
                    break;
                case "WeatherStation":
                    this.WeatherStationRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
                    break;
                case "WeatherReport":
                    this.WeatherReportRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
                    break;
                default:
                    list = new List<TRecord>();
                    break;
            };

            // Delay to demonstrate Async Programming
            await Task.Delay(200);
            return list; ;
        }

        public override async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(FilterListCollection filterList)
        {
            var list = await this.GetRecordListAsync<TRecord>();
            if (filterList != null && filterList.Count > 0)
            {
                foreach (var filter in filterList)
                {
                    var x = typeof(TRecord).GetProperty(filter.FieldName);
                    list = list.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
                }
            }
            else if (filterList is null || filterList.OnlyLoadIfFilters)
                list = new List<TRecord>();
            return list;
        }

        public override Task<int> GetRecordListCountAsync<TRecord>()
        {
            var count = this.GetRecordType<TRecord>() switch
            {
                "WeatherForecast" => this.WeatherForecastRecords.Count,
                "WeatherStation" => this.WeatherStationRecords.Count,
                "WeatherReport" => this.WeatherReportRecords.Count,
                _ => 0
            };
            return Task.FromResult(count);
        }

        public override async Task<List<string>> GetDistinctListAsync<TLookup>(string fieldName)
        {
            var list = new List<string>();
            var baselist = await this.GetRecordListAsync<TLookup>();
            var x = typeof(TLookup).GetProperty(fieldName);
            if (baselist != null && x != null)
            {
                var fulllist = baselist.Select(item => x.GetValue(item).ToString()).ToList();
                list = fulllist.Distinct().ToList();
            }
            return list ?? new List<string>();
        }

        public override async Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>()
        {
            var list = new SortedDictionary<int, string>();
            var baselist = await this.GetRecordListAsync<TLookup>();
            baselist.ForEach(item => list.Add(item.ID, item.DisplayName));
            return list;
        }


        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<TRecord> GetRecordAsync<TRecord>(int id)
        {
            switch (this.GetRecordType<TRecord>())
            {
                case "WeatherForecast":
                    {
                        var rec = this.WeatherForecastRecords.FirstOrDefault(item => item.ID == id);
                        return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
                    }
                case "WeatherStation":
                    {
                        var rec = this.WeatherStationRecords.FirstOrDefault(item => item.ID == id);
                        return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
                    }
                case "WeatherReport":
                    {
                        var rec = this.WeatherReportRecords.FirstOrDefault(item => item.ID == id);
                        return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
                    }
                default:
                    return Task.FromResult(default(TRecord));
            };
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record)
        {
            switch (this.GetRecordType<TRecord>())
            {
                case "WeatherForecast":
                    {
                        var delrec = this.WeatherForecastRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherForecastRecords.Remove(delrec);
                        var rec = (DbWeatherForecast)Convert.ChangeType(record, typeof(DbWeatherForecast));
                        this.WeatherForecastRecords.Add(rec);
                        break;
                    }
                case "WeatherStation":
                    {
                        var delrec = this.WeatherStationRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherStationRecords.Remove(delrec);
                        var rec = (DbWeatherStation)Convert.ChangeType(record, typeof(DbWeatherStation));
                        this.WeatherStationRecords.Add(rec);
                        break;
                    }
                case "WeatherReport":
                    {
                        var delrec = this.WeatherReportRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherReportRecords.Remove(delrec);
                        var rec = (DbWeatherReport)Convert.ChangeType(record, typeof(DbWeatherReport));
                        this.WeatherReportRecords.Add(rec);
                        break;
                    }
                default:
                    break;
            };
            return Task.FromResult(new DbTaskResult());
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record)
        {
            switch (this.GetRecordType<TRecord>())
            {
                case "WeatherForecast":
                    {
                        var rec = (DbWeatherForecast)Convert.ChangeType(record, typeof(DbWeatherForecast));
                        this.WeatherForecastRecords.Add(rec);
                        break;
                    }
                case "WeatherStation":
                    {
                        var rec = (DbWeatherStation)Convert.ChangeType(record, typeof(DbWeatherStation));
                        this.WeatherStationRecords.Add(rec);
                        break;
                    }
                case "WeatherReport":
                    {
                        var rec = (DbWeatherReport)Convert.ChangeType(record, typeof(DbWeatherReport));
                        this.WeatherReportRecords.Add(rec);
                        break;
                    }
                default:
                    break;
            };
            return Task.FromResult(new DbTaskResult());
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record)
        {
            switch (this.GetRecordType<TRecord>())
            {
                case "WeatherForecast":
                    {
                        var delrec = this.WeatherForecastRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherForecastRecords.Remove(delrec);
                        break;
                    }
                case "WeatherStation":
                    {
                        var delrec = this.WeatherStationRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherStationRecords.Remove(delrec);
                        break;
                    }
                case "WeatherReport":
                    {
                        var delrec = this.WeatherReportRecords.FirstOrDefault(item => item.ID == record.ID);
                        this.WeatherReportRecords.Remove(delrec);
                        break;
                    }
                default:
                    break;
            };
            return Task.FromResult(new DbTaskResult());
        }


        private string GetRecordType<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            return rec.RecordInfo.RecordName ?? string.Empty;
        }
    }
}
