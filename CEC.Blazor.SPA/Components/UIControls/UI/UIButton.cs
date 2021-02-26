using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a row
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UIButton : UIBase
    {

        /// <summary>
        /// Property setting the button HTML attribute Type
        /// </summary>
        [Parameter] public string ButtonType { get; set; } = "button";

        /// <summary>
        /// Boolena Property to set the Disabled attribute on the control
        /// </summary>
        [Parameter] public bool Disabled { get; set; }

        /// <summary>
        /// Override the CssName
        /// </summary>
        protected override string _BaseCss => "btn";

        /// <summary>
        /// Override the Tag
        /// </summary>
        protected override string _Tag => "button";

        /// <summary>
        /// Callback for a button click event
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> ClickEvent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, this._Tag);
            builder.AddMultipleAttributes(1, this.AttributesToRender);
            builder.AddAttribute(1, "type", this.ButtonType);
            if (!string.IsNullOrWhiteSpace(this._CssClass)) builder.AddAttribute(2, "class", this._CssClass);
            if (this.Disabled) builder.AddAttribute(3, "disabled");
            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
            builder.AddContent(5, ChildContent);
            builder.CloseElement();
        }

        /// <summary>
        /// Event handler for button click
        /// </summary>
        /// <param name="e"></param>
        protected void ButtonClick(MouseEventArgs e) => this.ClickEvent.InvokeAsync(e);
    }
}
