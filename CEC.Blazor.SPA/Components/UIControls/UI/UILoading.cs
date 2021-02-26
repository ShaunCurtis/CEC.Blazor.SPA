using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// A UI component that displays a spinner
    /// </summary>
    public class UILoading : UIBase
    {

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", this._CssClass);
            builder.AddMultipleAttributes(1, this.AttributesToRender);
            builder.OpenElement(4, "button");
            builder.AddAttribute(5, "class", "btn btn-primary");
            builder.AddAttribute(6, "type", "button");
            builder.AddAttribute(7, "disabled", "disabled");
            builder.OpenElement(8, "span");
            builder.AddAttribute(9, "class", "spinner-border spinner-border-sm pr-2");
            builder.AddAttribute(10, "role", "status");
            builder.AddAttribute(11, "aria-hidden", "true");
            builder.CloseElement();
            builder.AddContent(12, "  Loading...");
            builder.CloseElement();
            builder.CloseElement();
        }
    }
}
