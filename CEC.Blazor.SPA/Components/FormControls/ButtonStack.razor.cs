using CEC.Blazor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;


namespace CEC.Blazor.SPA.Components.FormControls
{
    public partial class ButtonStack : BaseBlazorComponent
    {
        [Parameter]
        public string ButtonCss { get; set; } = "btn btn-secondary";

        [Parameter]
        public string BaseCss { get; set; } = "btn-group-vertical btn-group-sm";
        
        [Parameter]
        public SortedDictionary<Guid, string> OptionList { get; set; } = new SortedDictionary<Guid, string>();

        [Parameter]
        public EventCallback<Guid> DeSelect { get; set; }

        public async void ReRender() => await this.RenderAsync();

        private void ButtonClick(Guid guid) => this.DeSelect.InvokeAsync(guid);

    }
}
