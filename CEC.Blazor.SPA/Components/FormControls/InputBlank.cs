using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace CEC.Blazor.SPA.Components.FormControls
{
    /// <summary>
    /// A display only Input box for formatted text
    /// </summary>
    public class InputBlank : Component
    {
        /// <summary>
        /// The string value that will be displayed
        /// </summary>
        [CascadingParameter]
        public EditContext EContext { get; set; }

        protected override Task OnRenderAsync(bool firstRender)
        {
            var x = true;
            return base.OnRenderAsync(firstRender);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Blank");
            builder.CloseElement();
        }

    }
}
