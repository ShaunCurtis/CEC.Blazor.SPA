/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using CEC.Blazor.Extensions;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Grid Column
    /// </summary>

    public class UIGridTableEditColumn<TRecord> : UIComponent where TRecord : class, IDbRecord<TRecord>, new()
    {
        /// <summary>
        /// Cascaded UIGridTable
        /// </summary>
        [CascadingParameter]
        public UICardGrid<TRecord> Card { get; set; }

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        /// <summary>
        /// HTML Colspan
        /// </summary>
        [Parameter]
        public int ColumnSpan { get; set; }

        protected override string _Tag => "td";

        /// <summary>
        /// Inherited
        /// </summary>
        protected override string _CssClass => this.CleanUpCss($"grid-col text-right {this._AddOnCss}");

        /// <summary>
        /// Button CSS
        /// </summary>
        private string _BadgeCss => "badge badge-primary p-2 text-white cursor-hand";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, this._Tag);
            builder.AddAttribute(1, "class", this._CssClass);
            if (this.ColumnSpan > 1) 
                builder.AddAttribute(2, "colspan", this.ColumnSpan);
            builder.OpenElement(3, "a");
            builder.AddAttribute(4, "class", this._BadgeCss);
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.ShowEditor(this.RecordID))));
            builder.AddContent(6, "Edit");
            builder.CloseElement();
            builder.CloseElement();
        }

        protected override bool? _Display 
            => this.RecordID > 0 && this.UIWrapper.Properties.Get<bool>("ShowEdit") && this.Card != null;
    }
}
