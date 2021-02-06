/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CEC.Blazor.Services
{
    public class BaseWASMDataService<TRecord, TContext> :
        BaseDataService<TRecord, TContext>,
        IDataService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {

        public BaseWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration) => this.HttpClient = httpClient;

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TRecord> GetRecordAsync(int id)
        {
            //return await this.HttpClient.GetFromJsonAsync<DbWeatherForecast>($"weatherforecast/getrec?id={id}");
            var response = await this.HttpClient.PostAsJsonAsync($"{RecordInfo.RecordName}/read", id);
            var result = await response.Content.ReadFromJsonAsync<TRecord>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new()
        {

            var recordname = typeof(TLookup).Name.Replace("Db", "", System.StringComparison.CurrentCultureIgnoreCase);
            return await this.HttpClient.GetFromJsonAsync<SortedDictionary<int, string>>($"{recordname}/lookuplist");
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetFilteredRecordListAsync(FilterListCollection filterList)
        {
            var response = await this.HttpClient.PostAsJsonAsync<FilterListCollection>($"{RecordInfo.RecordName}/filteredlist", filterList);
            var result = await response.Content.ReadFromJsonAsync<List<TRecord>>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync() => await this.HttpClient.GetFromJsonAsync<int>($"{RecordInfo.RecordName}/count");

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync(TRecord record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{RecordInfo.RecordName}/update", record);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> CreateRecordAsync(TRecord record)
        {
            var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{RecordInfo.RecordName}/create", record);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> DeleteRecordAsync(int id)
        {
            var response = await this.HttpClient.PostAsJsonAsync<int>($"{RecordInfo.RecordName}/update", id);
            var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            return result;
        }
    }
}
