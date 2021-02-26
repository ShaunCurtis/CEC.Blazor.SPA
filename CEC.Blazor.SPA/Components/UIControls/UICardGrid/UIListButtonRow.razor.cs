using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public partial class UIListButtonRow : UICardListBase
    {
        public bool ShowButtons => true;

        public bool ShowAdd =>  true;

        [Parameter]
        public bool IsPagination { get; set; } = true;

        [Parameter]
        public RenderFragment Paging { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

    }
}
