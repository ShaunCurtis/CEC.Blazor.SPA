/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CEC.Blazor.Data
{

    /// <summary>
    /// A class to surface data stored in the underlying RecordCollection
    /// Ufor theprovides an object with the data that the Editor uses and a Validator can be applied to
    /// The properties point to the data stored in the underlying RecordCollection
    /// </summary>
    public abstract class RecordEditData : IRecordEditData
    {
        #region Public

        /// <summary>
        /// Public bool to expose Validation state of dataset
        /// </summary>
        public bool IsValid => !Trip;

        /// <summary>
        /// Public bool to expose Edit State of dataset
        /// </summary>
        public bool IsDirty => this.RecordValues?.IsDirty ?? false;

        /// <summary>
        /// Public bool to expose Load state of the class instance
        /// </summary>
        public bool IsLoaded => this.EditContext != null && this.RecordValues != null;

        /// <summary>
        /// New Method
        /// </summary>
        /// <param name="collection">Record Collection for the current record</param>
        public RecordEditData(RecordCollection collection)
        {
            Debug.Assert(collection != null);

            if (collection is null)
            {
                throw new InvalidOperationException($"{nameof(RecordEditData)} requires a valid {nameof(RecordCollection)} object");
            }
            else
            {
                this.RecordValues = collection;
                this.LoadValidationActions();
            }
        }

        /// <summary>
        /// Method called to validate the Dataset
        /// </summary>
        /// <returns>True if valid</returns>
        public bool Validate()
        {
            ValidationRequested(this, ValidationRequestedEventArgs.Empty);
            return IsValid;
        }

        /// <summary>
        /// Notification method to inform the class that the edit context has changed
        /// Called from the RecordEditform
        /// </summary>
        /// <param name="context">New Edit Context</param>
        /// <returns></returns>
        public Task NotifyEditContextChangedAsync(EditContext context)
        {
            // assign the Edit Contexct and State internally
            this.EditContext = context;
            if (this.IsLoaded)
            {
                // Get the Validation Message Store from the EditContext
                ValidationMessageStore = new ValidationMessageStore(EditContext);
                // Wire up to the Editcontext to service Validation Requests
                EditContext.OnValidationRequested += ValidationRequested;
            }
            Validate();
            return Task.CompletedTask;
        }

        #endregion

        #region Protected


        protected RecordCollection RecordValues { get; private set; } = new RecordCollection();

        protected bool Trip = false;

        protected List<Func<bool>> ValidationActions { get; } = new List<Func<bool>>();

        protected virtual void LoadValidationActions() { }

        protected ValidationMessageStore ValidationMessageStore;

        #endregion

        #region Private

        private EditContext EditContext { get; set; }

        private void ValidationRequested(object sender, ValidationRequestedEventArgs args)
        {
            if (ValidationMessageStore != null)
            {
                // clear the message store and trip wire and check we have Validators to run
                this.ValidationMessageStore.Clear();
                this.Trip = false;
                    foreach (var validator in this.ValidationActions)
                    {
                            // invoke the action - defined as a func<bool> and trip if validation failed (false)
                            if (!validator.Invoke()) this.Trip = true;
                    }
                EditContext.NotifyValidationStateChanged();
            }
        }

        #endregion
    }
}
