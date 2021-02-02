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
    public class FactoryWASMDataService<TContext> :
        FactoryDataService<TContext>
        where TContext : DbContext
    {

        public FactoryWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration) 
            => this.HttpClient = httpClient;

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new TRecord();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/read", id);
                result = await response.Content.ReadFromJsonAsync<TRecord>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<SortedDictionary<int, string>> GetLookupListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new SortedDictionary<int, string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                result = await this.HttpClient.GetFromJsonAsync<SortedDictionary<int, string>>($"{recName}/lookuplist");
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new List<string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/distinctlist", fieldName);
                result = await response.Content.ReadFromJsonAsync<List<string>>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new List<TRecord>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<FilterList>($"{recName}/filteredlist", (FilterList)filterList);
                result = await response.Content.ReadFromJsonAsync<List<TRecord>>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = 0;
            if (TryGetRecordName<TRecord>(out string recName))
                result = await this.HttpClient.GetFromJsonAsync<int>($"{recName}/count"); 
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/update", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/create", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbTaskResult> DeleteRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<int>($"{recName}/delete", id);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }
    }
}
