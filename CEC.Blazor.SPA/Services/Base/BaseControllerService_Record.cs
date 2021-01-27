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
    /// Holds all the code for Record CRUD Operations
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public abstract partial class BaseControllerService<TRecord, TContext> :
        IDisposable,
        IControllerPagingService<TRecord>,
        IControllerService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext

    {
        #region Properties

        /// <summary>
        /// Unique ID for the Service
        /// Helps in debugging
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The Current Record
        /// Logic ensures a record always exists and the Shadow copy is up to date
        /// </summary>
        public TRecord Record
        {
            get
            {
                this._Record ??= new TRecord();
                return this._Record;
            }

            set => this._Record = value;
        }

        /// <summary>
        /// The actual Current Record
        /// </summary>
        protected TRecord _Record { get; private set; }

// TODO Get rid if Shadow Record
        /// <summary>
        /// Shadow Copy of the Current Record
        /// Should always be an unaltered copy of what's in the database
        /// Only used by Editors
        /// </summary>
        public TRecord ShadowRecord { set; get; } = new TRecord();

        // TODO Rename
        /// <summary>
        /// Collection of Record Values for Editor
        /// </summary>
        public RecordCollection RecordValues { get; } = new RecordCollection();

        /// <summary>
        /// Property to expose the Record ID
        /// Implemented in inherited classes with error checking for Record Exists
        /// </summary>
        public virtual int RecordID => this.Record?.ID ?? -1;

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

        /// <summary>
        /// Method to trigger a Record Changed Event
        /// </summary>
        public virtual void TriggerRecordChangedEvent(object sender) => this.RecordHasChanged?.Invoke(sender, EventArgs.Empty);


        #endregion

        #region Public Methods

        public virtual Task<bool> GetNewRecordAsync()
        {
            this.Record = new TRecord();
            this.ShadowRecord = new TRecord();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Async Gets a record from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<bool> GetRecordAsync(int? id, bool refresh = false)
        {
            if (this.IsService)
            {
                if (id != this.RecordID || refresh)
                {
                    if (id == 0)
                    {
                        this.Record = new TRecord();
                    }
                    else this.Record = await this.Service.GetRecordAsync(id ?? 0);
                    this.Record ??= new TRecord();
                    if (!this.IsRecords) this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
                    this.SetDirtyState(false);
                    this.TriggerRecordChangedEvent(this);
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
                    else this.Record = await this.Service.GetRecordAsync(guid);
                    this.Record ??= new TRecord();
                    if (!this.IsRecords) this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
                    this.SetDirtyState(false);
                    this.TriggerRecordChangedEvent(this);
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
            if (record is null) record = Record.GetFromProperties(this.RecordValues);
            if (record.ID > 0) return await UpdateRecordAsync(record);
            else return await AddRecordAsync();
        }

        /// <summary>
        /// Updates a record in the database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> UpdateRecordAsync(TRecord record = null)
        {
            if (record is null) record = Record.GetFromProperties(this.RecordValues);
            this.TaskResult = await this.Service.UpdateRecordAsync(record);
            if (this.TaskResult.IsOK)
            {
                this.Record = record;
                this.SetDirtyState(false);
                this.TriggerRecordChangedEvent(this);
                this.TriggerListChangedEvent(this);
            }
            return TaskResult.IsOK;
        }

        /// <summary>
        /// Adds a record to the Database
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> AddRecordAsync(TRecord record = null)
        {
            if (record is null) record = Record.GetFromProperties(this.RecordValues);
            this.TaskResult = await this.Service.CreateRecordAsync(record);
            if (this.TaskResult.IsOK)
            {
                this.Record = await this.Service.GetRecordAsync(this.TaskResult.NewID);
                this.SetDirtyState(false);
                this.TriggerRecordChangedEvent(this);
                this.TriggerListChangedEvent(this);
            }
            return TaskResult.IsOK && this.IsRecord;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method to set the state to dirty
        /// </summary>
        protected void SetDirtyState(bool isdirty = true)
        {
            this.IsClean = !isdirty;
            if (isdirty) this.OnDirty?.Invoke(this, EventArgs.Empty);
            else this.OnClean?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
