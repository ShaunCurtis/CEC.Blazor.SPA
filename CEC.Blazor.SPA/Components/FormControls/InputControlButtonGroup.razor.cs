using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace CEC.Blazor.SPA.Components.FormControls
{
    /// <summary>
    /// Form Control to display an Enum field as a button Group
    /// </summary>
    public partial class InputControlButtonGroup : InputBase<int>
    {
        [Parameter]
        public string SelectedButtonCss { get; set; } = "btn btn-info";

        [Parameter]
        public string ButtonCss { get; set; } = "btn btn-outline-secondary";

        [Parameter]
        public string BaseCss { get; set; } = "btn-group btn-group-sm";
        [Parameter]
        public SortedDictionary<int, string> OptionList { get; set; } = new SortedDictionary<int, string>();

        protected override string FormatValueAsString(int value) => value.ToString();

        protected override bool TryParseValueFromString(string value, out int result, out string validationErrorMessage)
        {
            if (!int.TryParse(value, out result)) result = 0;
            validationErrorMessage = null;
            return true;
        }

        protected string GetButtonCss(int value) => value == this.Value ? this.SelectedButtonCss : this.ButtonCss;

        protected void Switch(int value)
        {
            this.CurrentValue = value;
        }
    }
}
