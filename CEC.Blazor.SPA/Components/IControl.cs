using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components
{
    public interface IControl
    {
        [CascadingParameter] protected ViewManager ViewManager { get; set; }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected async void OpenModalAsync<TForm>(int id) where TForm : IComponent
        {
            var modalOptions = new ModalOptions()
            {
                HideHeader = true
            };
            modalOptions.Parameters.Add("ModalBodyCSS", "p-0");
            modalOptions.Parameters.Add("ModalCSS", "modal-xl");
            modalOptions.Parameters.Add("ID", id);
            await this.ViewManager.ShowModalAsync<TForm>(modalOptions);
        }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected async void OpenModalAsync<TForm>(ModalOptions modalOptions) where TForm : IComponent
        {
            await this.ViewManager.ShowModalAsync<TForm>(modalOptions);
        }
    }
}
