using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections;
using System.Data;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper for UI Cascading
    /// </summary>

    public class UIWrapper : UIBase
    {
        /// <summary>
        /// UIOptions object to cascade
        /// </summary>
        [Parameter]
        public PropertyCollection Properties { get; set; } = new PropertyCollection();

        /// <summary>
        /// OnView Action Delegate to cascade
        /// </summary>
        [Parameter]
        public Action<int> OnView { get; set; }

        /// <summary>
        /// OnEdit Action Delegate to cascade
        /// </summary>
        [Parameter]
        public Action<int> OnEdit { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<UIWrapper>>(0);
            builder.AddAttribute(1, "Value", this);
            if (this.ChildContent != null) builder.AddAttribute(2, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }
}
