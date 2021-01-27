/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// public Property of DbTaskResult set when a CRUD operation is called
        /// The UI can build an alert/confirmation method from the information provided
        /// </summary>
        public DbTaskResult TaskResult { get; protected set; } = new DbTaskResult();

        /// <summary>
        /// The record Configuration - read from the data service
        /// </summary>
        public DbRecordInfo RecordInfo => new TRecord().RecordInfo;

        /// <summary>
        /// Boolean Property used to check if the Data Service is set
        /// </summary>
        public bool IsService => this.Service != null;

        #endregion

        #region protected/private properties

        /// <summary>
        /// Corresponding Data Service for Type T
        /// </summary>
        protected IFactoryDataService<TDbContext> Service { get; set; }

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        protected IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Access to the Navigation Manager
        /// </summary>
        protected NavigationManager NavManager { get; set; }

        #endregion

        #region Events

        #endregion

        public FactoryControllerService(IConfiguration configuration, NavigationManager navigationManager, IFactoryDataService<TDbContext> dataService)
        {
            this.Service = dataService;
            this.NavManager = navigationManager;
            this.AppConfiguration = configuration;
            this.DefaultSortColumn = "ID";
            this.RecordValueCollection.FieldValueChanged = SetDirtyState;
        }

        /// <summary>
        /// Method to reset the Service to New
        /// May need to overridden in more complex services
        /// Called in edit mode when user navigates to New
        /// </summary>
        public virtual Task Reset()
        {
            this.FilterList = new FilterList();
            this.Record = new TRecord();
            this.Records = new List<TRecord>();
            this.PagedRecords = null;
            this.TriggerListChangedEvent(this);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pseudo Dispose Event - not currently used as Services don't have a Dispose event
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
