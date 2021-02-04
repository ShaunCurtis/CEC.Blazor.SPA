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
using System;

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
        public override async Task<TRecord> GetRecordAsync<TRecord>(int id)
        {
            var result = new TRecord();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/read?gid={Guid.NewGuid().ToString("D")}", id);
                result = await response.Content.ReadFromJsonAsync<TRecord>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<SortedDictionary<int, string>> GetLookupListAsync<TRecord>()
        {
            var result = new SortedDictionary<int, string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                result = await this.HttpClient.GetFromJsonAsync<SortedDictionary<int, string>>($"{recName}/lookuplist?gid={Guid.NewGuid().ToString("D")}");
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName)
        {
            var result = new List<string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/distinctlist?gid={Guid.NewGuid().ToString("D")}", fieldName);
                result = await response.Content.ReadFromJsonAsync<List<string>>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList)
        {
            var result = new List<TRecord>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<FilterList>($"{recName}/filteredlist?gid={Guid.NewGuid().ToString("D")}", (FilterList)filterList);
                result = await response.Content.ReadFromJsonAsync<List<TRecord>>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public override async Task<List<TRecord>> GetRecordListAsync<TRecord>()
        {
            var result = new List<TRecord>();
            if (TryGetRecordName<TRecord>(out string recName))
                result = await this.HttpClient.GetFromJsonAsync<List<TRecord>>($"{recName}/list?gid={Guid.NewGuid().ToString("D")}");
            return result;
        }


        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public override async Task<int> GetRecordListCountAsync<TRecord>()
        {
            var result = 0;
            if (TryGetRecordName<TRecord>(out string recName))
                result = await this.HttpClient.GetFromJsonAsync<int>($"{recName}/count?gid={Guid.NewGuid().ToString("D")}"); 
            return result;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/update?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/create?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override async Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/delete?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }
    }
}
