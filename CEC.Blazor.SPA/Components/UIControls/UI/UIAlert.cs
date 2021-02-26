using CEC.Blazor.SPA.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public class UIAlert : UIBase
    {
        /// <summary>
        /// Alert to display
        /// </summary>
        [Parameter]
        public FormAlert Alert { get; set; } = new FormAlert();

        /// <summary>
        /// Property to override the colour CSS
        /// </summary>
        protected override string _BaseCss => this.Alert != null ? CleanUpCss($"alert {this.Alert.GetCode()}") : "alert";

        /// <summary>
        /// Override the content with the alert message
        /// </summary>
        protected override string _Content => this.Alert?.Message ?? string.Empty;

        /// <summary>
        /// Override the diaplay based on the Alert setting
        /// </summary>
        protected override bool? _Display
            => this.Alert?.IsAlert ?? false;
    }
}
