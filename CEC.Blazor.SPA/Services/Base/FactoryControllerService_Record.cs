/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Services
{
    /// <summary>
    /// Base implementation of a Controller Service
    /// Implements IFactoryControllerService & IControllerPagingService interfaces
    /// Holds the working record and the working recordset for TRecord,
    /// all the data associated with CRUD operations for TRecord
    /// and managing the workflow on TRecord
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract partial class FactoryControllerService<TRecord, TDbContext> :
        IDisposable,
        IControllerPagingService<TRecord>,
        IFactoryControllerService<TRecord, TDbContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TDbContext : DbContext
    {
        #region Properties

        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The Current Record
        /// Can be overridden but do so with caution - you need to implement all the necessary functionality
        /// </summary>
        public virtual TRecord Record
        {
            get => this._Record;
            protected set
            {
                if (value != this._Record)
                {
                    this._Record = value;
                    this.GetRecordValues();
                    this.SetDirtyState(false);
                    this.RecordHasChanged?.Invoke(value, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The actual Current Record
        /// </summary>
        protected TRecord _Record { get; private set; }

        /// <summary>
        /// Collection of Record Values for Editor - immutable once the obect created
        /// Reload by clearing and adding new values back in
        /// </summary>
        public RecordCollection RecordValueCollection { get; } = new RecordCollection();

        /// <summary>
        /// Property to expose the Record ID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public virtual int RecordID => this.Record?.ID ?? -1;

        /// <summary>
        /// Property to expose the Record GUID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public virtual Guid RecordGUID => this.Record?.GUID ?? Guid.Empty;

        /// <summary>
        /// Boolean Property to check if a real record exists 
        /// </summary>
        public virtual bool IsRecord => this.RecordID > -1;

        /// <summary>
        /// Boolean Property to check if a New record exists 
        /// </summary>
        public virtual bool IsNewRecord => this.RecordID == 0;

        /// <summary>
        /// Property exposing the current save state of the record 
        /// </summary>
        public bool IsClean { get; protected set; } = true;

        #endregion

        #region protected/private properties

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the record is edited and not saved
        /// </summary>
        public event EventHandler OnDirty;

        /// <summary>
        /// Event triggered when the record is saved
        /// </summary>
        public event EventHandler OnClean;

        /// <summary>
        /// Event triggered when the record has changed
        /// </summary>
        public event EventHandler RecordHasChanged;

        #endregion

        /// <summary>
        /// Async Method to get a New Record
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> SetToNewRecordAsync(TRecord record = null)
        {
            if (record != null) this.Record = record;
            else this.Record = new TRecord();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Resets the Record to new TRecord
        /// </summary>
        /// <returns></returns>
        public virtual Task ResetRecordAsync()
        {
            if (this.Record != null) this.SetToNewRecordAsync();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to set the state to dirty
        /// </summary>
        public void SetDirtyState(bool isdirty = true)
        {
            this.IsClean = !isdirty;
            if (isdirty) this.OnDirty?.Invoke(this, EventArgs.Empty);
            else this.OnClean?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Async Gets a record from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> GetRecordAsync(int id, bool refresh = false)
        {
            if (this.IsService)
            {
                if (id != this.RecordID || refresh)
                {
                    if (id == 0) this.Record = new TRecord();
                    else this.Record = (await this.Service.GetRecordAsync<TRecord>(id)) ?? new TRecord();
                }
            }
            return this.IsRecord;
        }


        /// <summary>
        /// Async Gets a record from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> GetRecordAsync(Guid guid, bool refresh = false)
        {
            if (this.IsService)
            {
                var currentguid = this.Record?.GUID ?? Guid.Empty;
                if (guid != currentguid || refresh)
                {
                    if (guid == Guid.Empty) this.Record = new TRecord();
                    else this.Record = (await this.Service.GetRecordAsync<TRecord>(guid)) ?? new TRecord();
                }
            }
            return this.IsRecord;
        }

        /// <summary>
        /// Saves a record - adds if new, otherwise updates the existing record
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> SaveRecordAsync(TRecord record = null)
        {
            // Get a new record from the RecordCollection
            if (record is null) record = Record.GetFromProperties(this.RecordValueCollection);
            // And save it
            if (record.ID > 0) this.TaskResult = await this.Service.UpdateRecordAsync(record);
            else this.TaskResult = await this.Service.CreateRecordAsync(record);

            // Check we saved
            if (this.TaskResult.IsOK)
            {
                // Get the record id from the TaskResult if we have a new record - RecordID == 0
                var id = this.RecordID != 0 ? this.RecordID : this.TaskResult.NewID;
                // Set the record, set the state, trigger a record changed event and reset the list(we will have changed displayed values) 
                this.Record = await this.Service.GetRecordAsync<TRecord>(id);
                await this.ResetListAsync();
            }
            return TaskResult.IsOK;
        }

        /// <summary>
        /// Updates a record in the database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> UpdateRecordAsync(TRecord record = null) => await SaveRecordAsync(record);

        /// <summary>
        /// Adds a record to the Database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> AddRecordAsync(TRecord record = null) => await SaveRecordAsync(record);

    }
}
