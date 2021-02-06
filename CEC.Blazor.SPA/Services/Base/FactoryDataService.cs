/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace CEC.Blazor.Services
{
    public abstract class FactoryDataService<TContext>: IFactoryDataService<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Guid for Service
        /// </summary>
        public Guid ServiceID { get; } = Guid.NewGuid();

        /// <summary>
        /// Access to the HttpClient
        /// </summary>
        public HttpClient HttpClient { get; set; } = null;

        public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        public FactoryDataService(IConfiguration configuration) => this.AppConfiguration = configuration;

        /// <summary>
        /// Gets the Record Name from TRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <returns></returns>
        protected string GetRecordName<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            return rec.RecordInfo.RecordName ?? string.Empty;
        }

        /// <summary>
        /// Gets the Record Name from TRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool TryGetRecordName<TRecord>(out string name) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            name = rec.RecordInfo.RecordName ?? string.Empty;
            return !string.IsNullOrWhiteSpace(name);

        }
        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public virtual Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a filtered Record List
        /// </summary>
        /// <returns></returns>
        public virtual Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(FilterListCollection filterList) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public virtual Task<TRecord> GetRecordAsync<TRecord>(Guid guid) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to get the current record count
        /// </summary>
        /// <returns></returns>
        public virtual Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(0);

        /// <summary>
        /// Method to update a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// method to add a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to get a dictionary of id/name for a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public virtual Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new()
            => Task.FromResult(new SortedDictionary<int, string>());

        /// <summary>
        /// Method to get a dictionary of id/name for a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public virtual Task<SortedDictionary<int, string>> GetLookupListAsync(string recordName)
            => Task.FromResult(new SortedDictionary<int, string>());

        /// <summary>
        /// Method to get a dictionary of distinct values for a field in a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public virtual Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<string>());

        /// <summary>
        /// Method to build the a list of SqlParameters for a CUD Stored Procedure
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        public virtual List<SqlParameter> GetSQLParameters<TRecord>(TRecord item, bool withid = false) where TRecord : class, new()
            => new List<SqlParameter>();


    }
}
