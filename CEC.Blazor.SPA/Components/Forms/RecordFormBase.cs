/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CEC.Blazor.Core;
using System;

namespace CEC.Blazor.SPA.Components.Forms
{
    /// <summary>
    /// Class to add Record Viewing functionality to the ControllerServiceFormBase component
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public class RecordFormBase<TRecord, TContext> :
        ControllerServiceFormBase<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// This Page/Component Title
        /// </summary>
        public virtual string PageTitle => (this.Service?.Record?.DisplayName ?? string.Empty).Trim();

        /// <summary>
        /// Boolean Property that checks if a record exists
        /// </summary>
        protected virtual bool IsRecord => this.Service?.IsRecord ?? false;

        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        protected virtual bool IsError { get => !this.IsRecord; }

        /// <summary>
        /// Used to determine if the page has display data i.e. it's not loading or in error
        /// </summary>
        protected virtual bool IsLoaded => !(this.Loading) && !(this.IsError);

        /// <summary>
        /// Property for the ID of the record to retrieve.
        /// Normally set by Routing e.g. /Farm/Edit/1
        /// </summary>
        [Parameter]
        public int? ID
        {
            get => this._ID;
            set => this._ID = (value is null) ? -1 : (int)value;
        }

        /// <summary>
        /// Not Null Version of the ID
        /// </summary>
        public int _ID { get; private set; }

        /// <summary>
        /// Overridden OnRenderAsync Component Event
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected async override Task OnRenderAsync(bool firstRender)
        {
            // On first load reset the Service record to make sure we're clean
            if (firstRender && this.IsService)
            {
                await this.Service.ResetRecordAsync();
                // Set up the Record Changed event to trigger a render event on the component
                this.Service.RecordHasChanged += this.OnRecordChangedAsync;
            }
            // Load the Record
            await this.LoadRecordAsync(firstRender);
            //  Call down the hierachy
            await base.OnRenderAsync(firstRender);
        }

        protected void Render_Async(object sender, EventArgs e) => this.RenderAsync();

        /// <summary>
        /// Method to Load Record
        /// Reloads the record if the ID has changed
        /// Can be overridden for more complex loads - as in the Workflow Controller
        /// </summary>
        /// <returns></returns>
        protected virtual async Task LoadRecordAsync(bool firstload = false, bool setLoading = true )
        {
            if (this.IsService)
            {
                // Set the Loading flag 
                if (setLoading) this.Loading = true;
                //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
                if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

                // Get the current record - this will check if the id is different from the current record and only update if it's changed
                await this.Service.GetRecordAsync(this._ID, firstload);

                // Set the error message - it will only be displayed if we have an error
                this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

                // Set the Loading flag
                if (setLoading) this.Loading = false;
                //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
            }
        }

        protected virtual async void OnRecordChangedAsync(object sender, EventArgs e) => await this.RenderAsync();

        protected override void Dispose(bool disposing)
        {
            if (this.IsService) this.Service.RecordHasChanged -= this.OnRecordChangedAsync;
            base.Dispose(disposing);
        }
    }
}
