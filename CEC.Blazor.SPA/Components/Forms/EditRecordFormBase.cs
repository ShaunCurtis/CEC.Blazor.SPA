using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using CEC.Blazor.Core;

namespace CEC.Blazor.SPA.Components.Forms
{
    public abstract class EditRecordFormBase<TRecord, TDbContext> :
        RecordFormBase<TRecord, TDbContext>
        where TRecord : class, IDbRecord<TRecord>, new()
       where TDbContext : DbContext
    {
        /// <summary>
        /// Boolean Property exposing the Service Clean state
        /// </summary>
        public bool IsClean => this.Service?.IsClean ?? true;

        /// <summary>
        /// EditContext for the component
        /// </summary>
        protected EditContext EditContext
        {
            get => this._EditContext;
            set
            {
                if (!value.Equals(_EditContext))
                {
                    var old = this._EditContext;
                    this._EditContext = value;
                    this.EditContextChangedAsync(old);

                    // Task.Run(() => this.EditContextChangedAsync(old));
                }
            }
        }

        private EditContext _EditContext = null;

        /// <summary>
        /// Property to concatinate the Page Title
        /// </summary>
        public override string PageTitle
        {
            get
            {
                if (this.IsNewRecord) return $"New {this.Service?.RecordInfo?.RecordDescription ?? "Record"}";
                else return $"{this.Service?.RecordInfo?.RecordDescription ?? "Record"} Editor";
            }
        }

        /// <summary>
        /// Boolean Property to determine if the record is new or an edit
        /// </summary>
        public bool IsNewRecord => this.Service?.RecordID == 0 ? true : false;

        /// <summary>
        /// property used by the UIErrorHandler component
        /// </summary>
        protected override bool IsError => !(this.IsRecord && this.EditContext != null);

        /// <summary>
        /// Inherited - Always call the base method first
        /// </summary>
        protected async override Task LoadRecordAsync(bool firstLoad = false, bool setContext = true, bool setLoading = true)
        {
            await Task.Yield();
            this.Loading = true;
            await base.LoadRecordAsync(firstLoad, false, setContext);
            //set up the Edit Context
            if (setContext) this.EditContext = new EditContext(this.Service.Record);
            if (firstLoad)
            {
                this.Service.OnDirty += this.OnRecordDirty;
                this.Service.OnClean += this.OnRecordClean;
            }
            this.Loading = false;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        protected void OnRecordDirty(object sender, EventArgs e)
        {
            this.ViewManager.LockView();
            this.AlertMessage.SetAlert("The Record isn't Saved", MessageType.Warning);
            InvokeAsync(this.Render);
        }

        protected void OnRecordClean(object sender, EventArgs e)
        {
            this.ViewManager.UnLockView();
            this.AlertMessage.ClearAlert();
            InvokeAsync(this.Render);
        }

        protected void onFieldChanged(object sender, EventArgs e)
        {
            this.Service.SetDirtyState(this.Service.RecordValueCollection.IsDirty);
        }

        /// <summary>
        /// Event handler for the RecordFromControls FieldChanged Event
        /// </summary>
        /// <param name="isdirty"></param>
        protected virtual void RecordFieldChanged(bool isdirty)
        {
            if (this.EditContext != null) this.Service.SetDirtyState(isdirty);
        }

        protected virtual void EditContextChangedAsync(EditContext oldcontext) { }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected virtual async Task<bool> Save()
        {
            var ok = false;
            // Validate the EditContext
            if (this.EditContext.Validate())
            {
                // Save the Record
                ok = await this.Service.SaveRecordAsync();
                if (ok)
                {
                    // Set the EditContext State
                    this.EditContext.MarkAsUnmodified();
                }
                // Set the alert message to the return result
                this.AlertMessage.SetAlert(this.Service.TaskResult);
                // Trigger a component State update - buttons and alert need to be sorted
                await RenderAsync();
            }
            else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", MessageType.Danger);
            return ok;
        }

        /// <summary>
        /// Save and Exit Method called from the Button
        /// </summary>
        protected virtual async void SaveAndExit()
        {
            if (await this.Save()) this.ConfirmExit();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void TryExit()
        {
            // Check if we are free to exit ot need confirmation
            if (this.IsClean) ConfirmExit();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected virtual void ConfirmExit()
        {
            // To escape a dirty component set IsClean manually and navigate.
            this.Service.SetDirtyState(false);
            // Sort the exit strategy
            this.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.IsService)
            {
                this.Service.OnDirty -= this.OnRecordDirty;
                this.Service.OnClean -= this.OnRecordClean;
            }
            base.Dispose(disposing);
        }
    }
}
