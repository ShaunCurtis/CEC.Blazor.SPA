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
    /// Implements IControllerService & IControllerPagingService interfaces
    /// Holds the working record and the working recordset for TRecord
    /// and all the data associated with CRUD operations for TRecord
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
        /// public Property of DbTaskResult set when a CRUD operation is called
        /// The UI can build an alert/confirmation method from the information provided
        /// </summary>
        public DbTaskResult TaskResult { get; protected set; } = new DbTaskResult();

        /// <summary>
        /// The record Configuration - read from the data service
        /// </summary>
        public DbRecordInfo RecordInfo => this.Service?.RecordInfo ?? new DbRecordInfo();

        /// <summary>
        /// Boolean Property used to check if the Data Service is set
        /// </summary>
        public bool IsService => this.Service != null;

        #endregion

        #region protected/private properties

        /// <summary>
        /// Corresponding Data Service for Type T
        /// </summary>
        protected IDataService<TRecord, TContext> Service { get; set; }

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        protected IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Access to the Navigation Manager
        /// </summary>
        protected NavigationManager NavManager { get; set; }

        #endregion


        public BaseControllerService(IConfiguration configuration, NavigationManager navigationManager)
        {
            this.NavManager = navigationManager;
            this.AppConfiguration = configuration;
            this.DefaultSortColumn = "ID";
        }

        /// <summary>
        /// Method to reset the Service to New
        /// May need to overridden in more complex services
        /// Called in edit mode when user navigates to New
        /// </summary>
        public async virtual Task Reset()
        {
            this.FilterList = new FilterListCollection();
            this.Record = new TRecord();
            this.ShadowRecord = new TRecord();
            this.Records = new List<TRecord>();
            this.PagedRecords = null;
            this.BaseRecordCount = await this.Service.GetRecordListCountAsync();
            this.SetDirtyState(false);
        }

        /// <summary>
        /// Pseudo Dispose Event - not currently used as Services don't have a Dispose event
        /// </summary>
        public virtual void Dispose()
        {
        }

    }
}
