// Component based on EditForm with the following Copyright Notice

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

/// =================================
/// Changes Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Core;
using System.Data;
using CEC.Blazor.Data;

namespace CEC.Blazor.SPA.Components
{
    /// <summary>
    /// Renders a Record Editor component.  Replaces a FormEditor which is too simplistic to handle record complexity. 
    /// </summary>
    public class RecordEditForm : Component
    {

        #region Public Properties/Fields

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="EditForm"/>.
        /// </summary>
        [Parameter] public RenderFragment<EditContext> ChildContent { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created <c>form</c> element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// Read only Property to get the Edit Context
        /// Only this component can change it
        /// </summary>
        public EditContext EditContext { 
            get => _editContext;
            private set
            {
                if (value != _editContext)
                {
                    if (_editContext != null) this.Model.NotifyEditContextChangedAsync(_editContext);
                    this._editContext = value;
                    this.EditContextChanged?.Invoke(value, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Specifies the top-level model object for the form. An edit context will
        /// be constructed for this model. If using this parameter, do not also supply
        /// a value for <see cref="EditContext"/>.
        /// </summary>
        [Parameter] public IRecordEditData Model { get; set; }

        /// <summary>
        /// UIOptions object to cascade
        /// </summary>
        [Parameter]
        public PropertyCollection Properties { get; set; } = new PropertyCollection();

        /// <summary>
        /// The Guid of the current record
        /// </summary>
        public Guid RecordToken { get; private set; }

        /// <summary>
        /// The current state of the record
        /// </summary>
        public bool IsDataDirty => this.Model?.IsDirty ?? false;

        /// <summary>
        /// The current state of the record validation
        /// </summary>
        public bool IsDataValidated => this.Model.IsValid;

        #endregion

        #region Events

        /// <summary>
        /// Event raised if the Edit Context is changed
        /// </summary>
        public event EventHandler EditContextChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Method to call to Notify the instance of a Record Change
        /// </summary>
        /// <param name="recordToken"></param>
        /// <param name="workflowState"></param>
        public void NotifyRecordChanged(Guid recordToken)
        {
            this.RecordToken = recordToken;
            // Get an edit context on the model if don't already have one
            if (Model != null && Model != _editContext?.Model)
                this.EditContext = new EditContext(Model!);
            this.Model.NotifyEditContextChangedAsync(this._editContext);
            this.RenderAsync();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Overridden OnRender Event
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override Task OnRenderAsync(bool firstRender)
        {
            if (Model == null)
                throw new InvalidOperationException($"{nameof(EditForm)} requires either a {nameof(Model)} " + $"parameter, or an {nameof(EditContext)} parameter, please provide one of these.");

            // Update _editContext if we don't have one yet
            if (Model != null && Model != _editContext?.Model)
                this.EditContext = new EditContext(Model!);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            Debug.Assert(_editContext != null);

            if (_editContext != null)
            {

                // Only rebuild if the _editContext changes
                builder.OpenRegion(_editContext.GetHashCode() + Model.GetHashCode());

                builder.OpenElement(0, "form");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.OpenComponent<CascadingValue<RecordEditForm>>(2);
                //builder.AddAttribute(3, "IsFixed", true);
                builder.AddAttribute(4, "Value", this);

                // Add the second cascade to the top element by buidling out a new render fragment
                builder.AddAttribute(5, "ChildContent", (RenderFragment)((builder1) =>
                {
                    builder1.OpenComponent<CascadingValue<EditContext>>(6);
                    //builder1.AddAttribute(7, "IsFixed", true);
                    builder1.AddAttribute(8, "Value", _editContext);
                    builder1.AddAttribute(8, "ChildContent", ChildContent?.Invoke(_editContext));
                    builder1.CloseComponent();
                }));

                builder.CloseComponent();
                builder.CloseElement();

                builder.CloseRegion();
            }
        }

        #endregion


        #region Private Properties/Fields

        /// <summary>
        /// Internal field for EditContext property
        /// </summary>
        private EditContext _editContext;

        #endregion

        #region Private Methods

        /// <summary>
        /// Event handler for validation state change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValidationStateChanged(object sender, ValidationStateChangedEventArgs e) => this.RenderAsync();

        #endregion
    }
}