using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.SPA.Components
{
    public class ModalEditForm : BaseBlazorComponent
    {

        [Parameter] public RenderFragment EditorContent { get; set; }

        [Parameter] public RenderFragment ButtonContent { get; set; }

        [Parameter] public RenderFragment LoadingContent { get; set; }

        [Parameter] public RenderFragment ErrorContent { get; set; }

        [Parameter] public bool IsLoaded { get; set; }

        [Parameter] public bool IsError { get; set; }

        [Parameter] public EditContext EditContext { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            //Debug.Assert(EditContext != null);
            if (this.IsLoaded)
            {
                // If EditContext changes, tear down and recreate all descendants.
                // This is so we can safely use the IsFixed optimization on CascadingValue,
                // optimizing for the common case where EditContext never changes.
                builder.OpenRegion(EditContext.GetHashCode());
                builder.OpenComponent<CascadingValue<EditContext>>(1);
                builder.AddAttribute(2, "IsFixed", true);
                builder.AddAttribute(3, "Value", this.EditContext);
                builder.AddAttribute(4, "ChildContent", this.EditorContent);
                builder.CloseComponent();
                builder.CloseRegion();
            }
            else if (this.IsError)
                builder.AddContent(10, this.ErrorContent);
            else
                builder.AddContent(10, this.LoadingContent);
            builder.AddContent(20, this.ButtonContent);
        }

    }
}
