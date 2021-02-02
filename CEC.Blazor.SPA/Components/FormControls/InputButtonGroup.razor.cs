﻿using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CEC.Blazor.SPA.Components.FormControls
{
    /// <summary>
    /// Form Control to display an Enum field as a button Group
    /// </summary>
    public partial class InputButtonGroup : InputBase<int>
    {
        [Parameter]
        public string SelectedButtonCss { get; set; } = "btn btn-info text-white";

        [Parameter]
        public string ButtonCss { get; set; } = "btn btn-outline-secondary";

        [Parameter]
        public string BaseCss { get; set; } = "btn-group btn-group-sm";
        [Parameter]
        public SortedDictionary<int, string> OptionList { get; set; } = new SortedDictionary<int, string>();

        protected string GetButtonCss(int value) => value == this.Value ? this.SelectedButtonCss : this.ButtonCss;

        protected void SwitchValue(int value)
            => this.CurrentValue = value;

        protected override bool TryParseValueFromString(string value, [MaybeNullWhen(false)] out int result, [NotNullWhen(false)] out string validationErrorMessage)
            => throw new NotSupportedException($"This component does not parse string inputs. Bind to the '{nameof(CurrentValue)}' property, not '{nameof(CurrentValueAsString)}'.");

    }
}