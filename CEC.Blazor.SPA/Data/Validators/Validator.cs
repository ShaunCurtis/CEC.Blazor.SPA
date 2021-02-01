﻿using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEC.Blazor.Data.Validators
{
    public abstract class Validator<T>
    {
        #region Public

        /// <summary>
        /// True if the values passed Validation
        /// </summary>
        public bool IsValid => !Trip;

        /// <summary>
        /// Tripwire for validation failure
        /// </summary>
        public bool Trip = false;

        /// <summary>
        /// Messages to display if validation fails
        /// </summary>
        public List<string> Messages { get; } = new List<string>();

        /// <summary>
        /// Class Contructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <param name="model"></param>
        /// <param name="validationMessageStore"></param>
        /// <param name="message"></param>
        public Validator(T value, string fieldName, object model, ValidationMessageStore validationMessageStore, string message)
        {
            this.FieldName = fieldName;
            this.Value = value;
            this.Model = model;
            this.ValidationMessageStore = validationMessageStore;
            this.DefaultMessage = string.IsNullOrWhiteSpace(message) ? this.DefaultMessage : message;
        }

        /// <summary>
        /// Method to Log the Validation to the Validation Store
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool Validate(string message = null)
        {
            if (!this.IsValid)
            {
                message ??= this.DefaultMessage;
                // Check if we've logged specific messages.  If not add the default message
                if (this.Messages.Count == 0) Messages.Add(message);
                //set up a FieldIdentifier and add the message to the Edit Context ValidationMessageStore
                var fi = new FieldIdentifier(this.Model, this.FieldName);
                this.ValidationMessageStore.Add(fi, this.Messages);
            }
            return this.IsValid;
        }

        #endregion

        #region Protected

        /// <summary>
        /// Name of the Field
        /// </summary>
        protected string FieldName { get; set; }

        /// <summary>
        /// Field Value
        /// </summary>
        protected T Value { get; set; }

        /// <summary>
        /// Default message to diplay if failed validation
        /// </summary>
        protected string DefaultMessage { get; set; } = "The value failed validation";

        /// <summary>
        /// Reference to the current Edit Context ValidationMessageStore
        /// </summary>
        protected ValidationMessageStore ValidationMessageStore { get; set; }

        protected object Model { get; set; }

        /// <summary>
        /// Method to add a message to the log
        /// </summary>
        /// <param name="message"></param>
        protected void LogMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) Messages.Add(message);
        }

        #endregion
    }
}
