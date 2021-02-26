using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Css Framework Html Component
    /// This is a base component implementing ControlBase - not a ComponentBase inherited class.
    /// Note that many of the parameter properties have a protected _property
    /// You can override the value used by setting the _property specifically in any derived classes
    /// The _property is the property actually used in the render process.
    /// e.g. protected override string _Tag => "div";
    /// will force the component tag to be a div. 
    /// </summary>

    public abstract class UIBase : BaseBlazorComponent, IDisposable
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created <c>form</c> element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Css for component - can be overridden and fixed in inherited components
        /// </summary>
        [Parameter]
        public virtual string Tag { get; set; } = "div";

        /// <summary>
        /// Property to set the HTML value if appropriate
        /// </summary>
        [Parameter]
        public string Value { get; set; } = "";

        #endregion

        #region Protected properties used by inheriting components

        /// <summary>
        /// Html attributes that need to be removed if set on the control
        /// default is only the class attribute
        /// </summary>
        protected List<string> UsedAttributes { get; set; } = new List<string>();


        /// <summary>
        /// Html tag for the control - default is a div.
        /// In general use css display to change the block behaviour
        /// </summary>
        protected virtual string _Tag => this.Tag;

        /// <summary>
        /// Property for fixing the base Css.  Base returns the Parameter Css, but can be overridden in inherited classes
        /// </summary>
        protected virtual string _BaseCss { get; set; }

        /// <summary>
        /// Property for fixing the Add On Css.  By default gets the Class value from the Additional Attributes
        /// </summary>
        protected virtual string _AddOnCss => AdditionalAttributes.TryGetValue("class", out object value) ? value.ToString() : string.Empty;

        /// <summary>
        /// Actual calculated Css string used in the component
        /// </summary>
        protected virtual string _CssClass => this.CleanUpCss($"{this._BaseCss} {this._AddOnCss}");

        /// <summary>
        /// Property to override the content of the component
        /// </summary>
        protected virtual string _Content => string.Empty;

        protected Dictionary<string, object> AttributesToRender
            => ClearDuplicateAttributes();

        #endregion

        #region Methods

        /// <summary>
        /// inherited
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, this._Tag);
            builder.AddMultipleAttributes(1, this.AttributesToRender);
            if (!string.IsNullOrWhiteSpace(this._CssClass)) builder.AddAttribute(2, "class", this._CssClass);
            if (!string.IsNullOrEmpty(this._Content)) builder.AddContent(3, (MarkupString)this._Content);
            else if (this.ChildContent != null) builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }

        /// <summary>
        /// Method to clean up the Additional Attributes
        /// </summary>
        protected Dictionary<string, object> ClearDuplicateAttributes()
        {
            var attributes = AdditionalAttributes.Keys.ToDictionary(_ => _, _ => AdditionalAttributes[_]);
            if (this.AdditionalAttributes != null && this.UsedAttributes != null)
            {
                foreach (var item in this.UsedAttributes)
                {
                    if (attributes.ContainsKey(item)) attributes.Remove(item);
                }
            }
            return attributes;
        }

        /// <summary>
        /// Method to clean up the Css - remove leading and trailing spaces and any multiple spaces
        /// </summary>
        /// <param name="css"></param>
        /// <returns></returns>
        protected string CleanUpCss(string css)
        {
            while (css.Contains("  ")) css = css.Replace("  ", " ");
            return css.Trim();
        }

        public virtual void Dispose() { }

        #endregion
    }
}
