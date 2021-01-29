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
        /// Property for referencing to the RecordEditForm component when rendered
        /// </summary>
        protected RecordEditForm RecordEditForm { get; set; }

        /// <summary>
        /// RecordEditData object used for edit control and Validation
        /// Should be assigned in defived classes at first render
        /// </summary>
        protected IRecordEditContext RecordEditorContext { get; set; }

        /// <summary>
        /// Boolean Property exposing the Service Clean state
        /// </summary>
        public bool IsClean => (this.RecordEditorContext?.IsClean ?? true);

        /// <summary>
        /// Boolean Validation checker for exposing last Validation check
        /// </summary>
        protected bool IsValidated => this.RecordEditorContext?.IsValid ?? true;

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
        protected override bool IsError => !(this.IsRecord);

        /// <summary>
        /// Inherited - Always call the base method first
        /// </summary>
        protected async override Task LoadRecordAsync(bool firstLoad = false, bool setLoading = true)
        {
            await Task.Yield();
            this.Loading = true;
            await base.LoadRecordAsync(firstLoad, false);
            this.RecordEditorContext.EditContextChanged += this.EditContextChanged;
            this.Loading = false;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        protected void onFieldChanged(object sender, EventArgs e)
        {
            if (this.Service.RecordValueCollection.IsDirty)
            {
                this.ViewManager.LockView();
                this.AlertMessage.SetAlert("The Record isn't Saved", MessageType.Warning);
            }
            else
            {
                this.ViewManager.UnLockView();
                this.AlertMessage.ClearAlert();
            }
            this.RenderAsync();
        }

        protected virtual void EditContextChanged(object sender, EditContextEventArgs e) 
        {
            //sort the field changed even handling - we need to lock the form if the RecordEditContect is dirty
         if (e.OldContext != null) 
                e.OldContext.OnFieldChanged -= onFieldChanged;
            if (e.NewContext != null)
                e.NewContext.OnFieldChanged += onFieldChanged;
        }

        private void OnValidationStateChanged(object sender, ValidationStateChangedEventArgs e) => this.RenderAsync();

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected virtual async Task<bool> Save()
        {
            var ok = false;
            // Validate the EditContext
            if (this.RecordEditorContext.EditContext.Validate())
            {
                // Save the Record
                ok = await this.Service.SaveRecordAsync();
                if (ok)
                {
                    // Set the EditContext State
                    this.RecordEditorContext.EditContext.MarkAsUnmodified();
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
    }
}
