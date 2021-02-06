/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    /// <summary>
    /// Interface definition for a Controller Service that uses the FactoryDataService
    /// Holds the working record and the working recordset for TRecord
    /// and all the data associated with CRUD operations for TRecord
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface IFactoryControllerService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Record Configuration Object - provides name and routing information for the DB Model Class
        /// </summary>
        public DbRecordInfo RecordInfo { get; }

        /// <summary>
        /// Collection of Record Values for Editor
        /// </summary>
        public RecordCollection RecordValueCollection { get; }

        /// <summary>
        /// Current Record - for CRUD Component Operations
        /// </summary>
        public TRecord Record { get; }

        /// <summary>
        /// Current List of Records - used in Listing Components 
        /// </summary>
        public List<TRecord> Records { get; set; }

        /// <summary>
        /// Property to get the default page size retrieved from the application Configuration
        /// </summary>
        public int DefaultPageSize { get; }

        /// <summary>
        /// Property to get current record count
        /// </summary>
        public int RecordCount => this.Records?.Count ?? 0;

        /// <summary>
        /// Boolean Property to check if a record exists
        /// </summary>
        public virtual bool IsRecord => this.Record != null && this.RecordID > -1;

        /// <summary>
        /// Boolean Property to check if a New record exists 
        /// </summary>
        public virtual bool IsNewRecord => this.IsRecord && this.RecordID == 0;

        /// <summary>
        /// Property to expose the Record ID.
        /// should be implemented to return 0 if the record is null
        /// </summary>
        public abstract int RecordID { get; }

        /// <summary>
        /// Property to expose the Record GUID.
        /// should be implemented to return 0 if the record is null
        /// </summary>
        public abstract Guid RecordGUID { get; }

        /// <summary>
        /// Property of DbTaskResult set when a CRUD operation is called
        /// The UI can build an alert/confirmation method from the information provided
        /// </summary>
        public DbTaskResult TaskResult { get; }

        /// <summary>
        /// Property exposing the current edit state of the record 
        /// </summary>
        public bool IsClean { get; }

        /// <summary>
        /// Filter List for applying to Records List
        /// </summary>
        public FilterListCollection FilterList { get; set; }

        /// <summary>
        /// Event raised when the Filter has Changed
        /// </summary>
        public event EventHandler FilterHasChanged;

        /// <summary>
        /// Event raised when the Record has Changed
        /// </summary>
        public event EventHandler RecordHasChanged;

        /// <summary>
        /// Event raised when the List has Changed
        /// </summary>
        public event EventHandler ListHasChanged;

        /// <summary>
        /// Event triggered when the record is edited and not saved
        /// </summary>
        public event EventHandler OnDirty;

        /// <summary>
        /// Event triggered when the record is saved
        /// </summary>
        public event EventHandler OnClean;

        /// <summary>
        /// Method to trigger a Record Changed Event
        /// </summary>
        public void TriggerFilterChangedEvent(object sender);

        /// <summary>
        /// Method to trigger a List Changed Event
        /// </summary>
        public void TriggerListChangedEvent(object sender);


        /// <summary>
        /// Method to Reset the Service to New condition
        /// </summary>
        public Task Reset();

        /// <summary>
        /// Method to set the state to clean
        /// </summary>
        public void SetDirtyState(bool isdirty);

        /// <summary>
        /// Method to Update or Add the Database Record
        /// Builds the record from the RecordCollection if one is not provided
        /// </summary>
        /// <returns></returns>
        public Task<bool> SaveRecordAsync(TRecord record = null);

        /// <summary>
        /// Method to Update the Database Record
        /// </summary>
        /// <returns></returns>
        public Task<bool> UpdateRecordAsync(TRecord record = null) => Task.FromResult(false);

        /// <summary>
        /// Method to add a Record to the Database
        /// </summary>
        /// <returns></returns>
        public Task<bool> AddRecordAsync(TRecord record = null) => Task.FromResult(false);

        /// <summary>
        /// Method to get a Record from the Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<bool> GetRecordAsync(int id, bool reload = false) => Task.FromResult(false);

        /// <summary>
        /// Method to get a new Record
        /// </summary>
        /// <returns></returns>
        public Task<bool> SetToNewRecordAsync(TRecord record = null) => Task.FromResult(false);

        /// <summary>
        /// Method to get a lookuplist for a Record of ID and DisplayName
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<SortedDictionary<int,string>> GetLookUpListAsync<TLookup>(string selectAllText = null) where TLookup : class, IDbRecord<TLookup>, new() => Task.FromResult(new SortedDictionary<int, string>());

        /// <summary>
        /// Method to get a lookup list of values for a Field in TLookup record
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public Task<SortedDictionary<object, string>> GetDistinctListAsync<TLookup>(string fieldName) where TLookup : class, IDbRecord<TLookup>, new() => Task.FromResult(new SortedDictionary<object, string>());

        /// <summary>
        /// Method to reset the record to new
        /// </summary>
        /// <returns></returns>
        public Task ResetRecordAsync();

        /// <summary>
        /// Method to load up the Paged Data to display
        /// </summary>
        public Task LoadPagingAsync();

        /// <summary>
        /// Method to get a page of data from the database - used during paging operations
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetDataPageAsync();

        /// <summary>
        /// Method to rest the list to empty
        /// </summary>
        /// <returns></returns>
        public Task ResetListAsync();

        /// <summary>
        /// Method to create a copy of the current record
        /// </summary>
        /// <returns></returns>
        public Task CopyRecordAsync() => Task.CompletedTask;

        /// <summary>
        /// Method to load any lookup Lists
        /// </summary>
        /// <returns></returns>
        public Task LoadLookups() => Task.CompletedTask;

    }
}
