/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Services;
using Microsoft.EntityFrameworkCore;
using CEC.Blazor.Core;
using CEC.Blazor.Data;
using System.Collections;
using System.Data;

namespace CEC.Blazor.SPA.Components.Forms
{
    /// <summary>
    /// Abstract Base Form for using with a IController Service
    /// Used by the higher level CRUD Forms
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class ControllerServiceFormBase<TRecord, TDbContext> :
        BlazorFormBase
        where TRecord : class, IDbRecord<TRecord>, new()
        where TDbContext : DbContext
    {

        /// <summary>
        /// Service with IFactoryDataRecordService Interface that corresponds to Type T
        /// Normally set as the first line in the Page OnInitialized event.
        /// </summary>
        public IFactoryControllerService<TRecord, TDbContext> Service { get; set; }

        /// <summary>
        /// HashTable Property to control various UI Settings
        /// Used as a cascadingparameter
        /// </summary>
        [Parameter] public PropertyCollection Properties { get; set; } = new PropertyCollection();

        /// <summary>
        /// The default alert used by all inherited components
        /// Used for Editor Alerts, error messages, ....
        /// </summary>
        [Parameter] public FormAlert AlertMessage { get; set; } = new FormAlert();

        /// <summary>
        /// Property with generic error message for the Page Manager 
        /// </summary>
        protected virtual string RecordErrorMessage { get; set; } = "The Application is loading the record.";

        /// <summary>
        /// Boolean check if the Service exists
        /// </summary>
        protected bool IsService { get => this.Service != null; }

    }
}
