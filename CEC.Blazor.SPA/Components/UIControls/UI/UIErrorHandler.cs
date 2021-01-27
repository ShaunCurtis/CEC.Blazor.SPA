using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace CEC.Blazor.SPA.Components.UIControls
{
    /// <summary>
    /// A UI component that only displays the Child Content when contwent loading is complete and there are no errors
    /// This removes the need for complex error checking - such as if a record or list exists - in the child content.
    /// Connect IsError and IsLoading to boolean properties in the parent and update them when loading is complete.
    /// </summary>
    public class UIErrorHandler : UIBase
    {
        /// <summary>
        /// Enum for the Control State
        /// </summary>
        public enum ControlState { Error = 0, Loading = 1, Loaded = 2, AccessDenied = 3 }

        /// <summary>
        /// Boolean Property that determines if the child content or an error message is diplayed
        /// </summary>
        [Parameter]
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Boolean Property that determines if the child content or an loading message is diplayed
        /// </summary>
        [Parameter]
        public bool IsLoading { get; set; } = true;

        [Parameter]
        public bool AccessDenied { get; set; } = false;

        [Parameter]
        public RenderFragment LoadingContent { get; set; }

        [Parameter]
        public RenderFragment ErrorContent { get; set; }

        [Parameter]
        public RenderFragment AccessDeniedContent { get; set; }

        /// <summary>
        /// Control State
        /// </summary>
        public ControlState State
        {
            get
            {
                if (IsError && !IsLoading) return ControlState.Error;
                else if ((!IsLoading) && !AccessDenied) return ControlState.Loaded;
                else if (AccessDenied) return ControlState.AccessDenied;
                else return ControlState.Loading;
            }
        }

        /// <summary>
        /// CSS Override
        /// </summary>
        protected override string _BaseCss => this.IsLoading ? "text-center p-3" : "label label-error m-2";

        /// <summary>
        /// Customer error message to display
        /// </summary>
        [Parameter]
        public string ErrorMessage { get; set; } = "An error has occured loading the content";


        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearDuplicateAttributes();
            switch (this.State)
            {
                case ControlState.Loading:
                    if (this.LoadingContent != null) builder.AddContent(1, this.LoadingContent);
                    else
                    {
                        builder.OpenElement(2, "div");
                        builder.AddAttribute(3, "class", this._Css);
                        builder.OpenElement(4, "button");
                        builder.AddAttribute(5, "class", "btn btn-primary");
                        builder.AddAttribute(6, "type", "button");
                        builder.AddAttribute(7, "disabled", "disabled");
                        builder.OpenElement(8, "span");
                        builder.AddAttribute(9, "class", "spinner-border spinner-border-sm pr-2");
                        builder.AddAttribute(10, "role", "status");
                        builder.AddAttribute(11, "aria-hidden", "true");
                        builder.CloseElement();
                        builder.AddContent(12, "  Loading...");
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                    break;
                case ControlState.Error:
                    if (this.ErrorContent != null) builder.AddContent(100, this.ErrorContent);
                    else
                    {
                        builder.OpenElement(101, "div");
                        builder.OpenElement(102, "span");
                        builder.AddAttribute(103, "class", this._Css);
                        builder.AddContent(104, ErrorMessage);
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                    break;
                case ControlState.AccessDenied:
                    if (this.AccessDeniedContent != null) builder.AddContent(200, this.AccessDeniedContent);
                    else
                    {
                        builder.OpenElement(201, "div");
                        builder.OpenElement(202, "span");
                        builder.AddAttribute(203, "class", this._Css);
                        builder.AddContent(304, "You currently don't have permissions to access this record");
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                    break;
                default:
                    builder.AddContent(401, ChildContent);
                    break;
            };
        }

    }
}
