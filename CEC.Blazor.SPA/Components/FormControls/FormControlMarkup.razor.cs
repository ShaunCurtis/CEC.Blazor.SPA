using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components;


namespace CEC.Blazor.SPA.Components.FormControls
{
    public partial class FormControlMarkup : Component
    {

        [Parameter]
        public string Value { get; set; }

        [Parameter]
        public string HtmlLink { get; set; } = string.Empty;

        [Parameter]
        public string LinkCss { get; set; } = string.Empty;

        [Parameter]
        public bool UsePre { get; set; } = false;

    }
}
