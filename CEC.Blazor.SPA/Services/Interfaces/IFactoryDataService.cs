/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
/// <summary>
/// Interface for a Factory Data Service
/// defines the CRUD operations Methods for interfacing with the DbContext
/// </summary>
/// <typeparam name="TContext"></typeparam>
    public interface IFactoryDataService<TContext> 
        where TContext : DbContext
    {
        public HttpClient HttpClient { get; set; }

        public IDbContextFactory<TContext> DBContext { get; set; }

        public IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a filtered Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<TRecord> GetRecordAsync<TRecord>(Guid guid) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to get the current record count
        /// </summary>
        /// <returns></returns>
        public Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(0);

                /// <summary>
        /// Method to update a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// method to add a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to get a dictionary of id/name for a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() 
            => Task.FromResult(new SortedDictionary<int,string>());

        /// <summary>
        /// Method to get a dictionary of distinct values for a field in a record type
        /// Used in Lookup Lists
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new() 
            => Task.FromResult(new List<string>());

        /// <summary>
        /// Method to build the a list of SqlParameters for a CUD Stored Procedure
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        public List<SqlParameter> GetSQLParameters<TRecord>(TRecord item, bool withid = false) where TRecord : class, new() 
            => new List<SqlParameter>();

    }
}
