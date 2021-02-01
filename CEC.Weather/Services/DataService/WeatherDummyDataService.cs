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
        IFactoryDataService<WeatherForecastDbContext>
    {

        /// <summary>
        /// internal Property to hold the dummy records for CRUD operations
        /// </summary>
        private List<DbWeatherForecast> WeatherForecastRecords { get; set; }

        private List<DbWeatherStation> WeatherStationRecords { get; set; }

        private List<DbWeatherReport> WeatherReportRecords { get; set; }

        public HttpClient HttpClient { get; set; }
        
        public IDbContextFactory<WeatherForecastDbContext> DBContext { get; set; }

        public IConfiguration AppConfiguration { get; set; }

        public WeatherDummyDataService(IConfiguration configuration) 
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
                Elevation = 28

            };
            this.WeatherStationRecords.Add(rec);
            rec = new DbWeatherStation()
            {
                ID = 2,
                Name = "Ross-on-Wye",
                Longitude = -1.2m,
                Latitude = 52.2m,
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
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
        {
            if (typeof(DbWeatherForecast).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                var rec = this.WeatherForecastRecords.FirstOrDefault(item => item.ID == id);
                return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
            }
            else if (typeof(DbWeatherStation).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                var rec = this.WeatherStationRecords.FirstOrDefault(item => item.ID == id);
                return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
            }
            else if (typeof(DbWeatherReport).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                var rec = this.WeatherReportRecords.FirstOrDefault(item => item.ID == id);
                return Task.FromResult((TRecord)Convert.ChangeType(rec, typeof(TRecord)));
            }
            else
                return Task.FromResult(default(TRecord));
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var list = new List<TRecord>();
            if (typeof(DbWeatherForecast).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                this.WeatherForecastRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
            }
            else if (typeof(DbWeatherStation).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                this.WeatherStationRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
            }
            else if (typeof(DbWeatherReport).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                this.WeatherReportRecords.ForEach(item => list.Add((TRecord)Convert.ChangeType(item, typeof(TRecord))));
            }

            // Delay to demonstrate Async Programming
            await Task.Delay(200);
            return list; ;
        }

        public async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList) where TRecord : class, IDbRecord<TRecord>, new() => await GetRecordListAsync<TRecord>();

        public Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            int count = 0;
            if (typeof(DbWeatherForecast).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                count = this.WeatherForecastRecords.Count;
            }
            else if (typeof(DbWeatherStation).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                count = this.WeatherStationRecords.Count;
            }
            else if (typeof(DbWeatherReport).GetTypeInfo().IsAssignableFrom(typeof(TRecord).GetTypeInfo()))
            {
                count = this.WeatherReportRecords.Count;
            }
            return Task.FromResult(count);
        }
        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record)
            where TRecord : class, IDbRecord<TRecord>, new()
        {
            return Task.FromResult(new DbTaskResult());
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record)
            where TRecord : class, IDbRecord<TRecord>, new()
        {
            return Task.FromResult(new DbTaskResult());
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync<TRecord>(int id) 
            where TRecord : class, IDbRecord<TRecord>, new()
        {
            return Task.FromResult(new DbTaskResult());
        }
    }
}
