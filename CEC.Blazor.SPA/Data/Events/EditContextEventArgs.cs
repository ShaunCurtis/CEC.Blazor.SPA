/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using Microsoft.AspNetCore.Components.Forms;
using System;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Event Args for a EditContext change event handler
    /// </summary>
    public class EditContextEventArgs : EventArgs
    {
        public EditContext OldContext { get; set; } = null;

        public EditContext NewContext { get; set; } = null;
    }
}
