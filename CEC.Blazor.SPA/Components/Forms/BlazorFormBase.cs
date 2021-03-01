/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.SPA.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Core
{
    /// <summary>
    /// Abstract base class for all forms it:
    /// - Impelements Scoped Services to provide Component based scoping of Scoped Services
    /// - Tracks the user if Authentication is enabled
    /// - provides basic exit and close methods from the form depending on when it is part of a view or a Modal Dialog
    /// - Implements IDisposable for displosing of event handlers
    /// - Implements IForm for labelling as a UI Form
    /// </summary>
    public abstract class BlazorFormBase : BlazorComponent, IForm, IDisposable
    {
        private IServiceScope _scope;


        /// <summary>
        /// Gets the scoped <see cref="IServiceProvider"/> that is associated with this component.
        /// </summary>
        protected IServiceProvider ScopedServices
        {
            get
            {
                if (ScopeFactory == null) throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
                if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
                _scope ??= ScopeFactory.CreateScope();
                return _scope.ServiceProvider;
            }
        }

        /// <summary>
        /// Scope Factory to manage Scoped Services
        /// </summary>
        [Inject]
        protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

        /// <summary>
        /// Cascaded Parameter if the Component is used in Modal mode
        /// </summary>
        [CascadingParameter]
        protected IModal ModalParent { get; set; }

        /// <summary>
        /// Boolean Property to check if this component is in Modal Mode
        /// </summary>
        public bool IsModal => this.ModalParent != null;

        /// <summary>
        /// Cascaded Authentication State Task from <CascadingAuthenticationState> in App
        /// </summary>
        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        /// <summary>
        /// Cascaded ViewManager
        /// </summary>
        [CascadingParameter] public ViewManager ViewManager { get; set; }

        /// <summary>
        /// Property holding the current user name
        /// Used in SessionState Service Management
        /// </summary>
        public string CurrentUser { get; protected set; }

        /// <summary>
        /// Guid string for user
        /// </summary>
        public string CurrentUserID { get; set; }

        /// <summary>
        /// Check if ViewManager exists
        /// </summary>
        public bool IsViewManager => this.ViewManager != null;

        /// <summary>
        /// UserName without the domain name
        /// </summary>
        public string CurrentUserName => (!string.IsNullOrEmpty(this.CurrentUser)) && this.CurrentUser.Contains("@") ? this.CurrentUser.Substring(0, this.CurrentUser.IndexOf("@")) : string.Empty;

        /// <summary>
        /// Gets a value determining if the component and associated services have been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// OnRenderAsync Method from Component
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected async override Task OnUpdateAsync(bool firstRender)
        {
            if (firstRender) {
                await GetUserAsync();
            }
            await base.OnUpdateAsync(firstRender);
        }

        /// <summary>
        /// Method to get the current user from the Authentication State
        /// </summary>
        protected async Task GetUserAsync()
        {
            if (this.AuthenticationStateTask != null)
            {
                var state = await AuthenticationStateTask;
                // Get the current user
                this.CurrentUser = state.User.Identity.Name;
                var x = state.User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"));
                this.CurrentUserID = x?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Exit method for the form
        /// If the form is a Modal Dialog it closes the Dialog
        /// Otherwise it changes the displayed view to the previously loaded view.
        /// </summary>
        public void Exit(ModalResult result)
        {
            if (IsModal) this.ModalParent.Close(result);
            else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
        }

        /// <summary>
        /// Exit method for the form
        /// If the form is a Modal Dialog it closes the Dialog
        /// Otherwise it changes the displayed view to the previously loaded view.
        /// </summary>
        public void Exit()
        {
            if (IsModal) this.ModalParent.Close(ModalResult.Exit());
            else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
        }

        /// <summary>
        /// Exit method for the form
        /// If the form is a Modal Dialog it closes the Dialog
        /// Otherwise it changes the displayed view to the previously loaded view.
        /// </summary>

        public void Cancel()
        {
            if (IsModal) this.ModalParent.Close(ModalResult.Cancel());
            else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
        }

        /// <summary>
        /// Exit method for the form
        /// If the form is a Modal Dialog it closes the Dialog
        /// Otherwise it changes the displayed view to the previously loaded view.
        /// </summary>
        public void OK()
        {
            if (IsModal) this.ModalParent.Close(ModalResult.OK());
            else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
        }


        /// <summary>
        /// IDisposable Interface
        /// </summary>
        async void IDisposable.Dispose()
        {
            if (!IsDisposed)
            {
                _scope?.Dispose();
                _scope = null;
                Dispose(disposing: true);
                await this.DisposeAsync(true);
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Method
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) { }


        /// <summary>
        /// Async Dispose event to clean up event handlers
        /// </summary>
        public virtual Task DisposeAsync(bool disposing) => Task.CompletedTask;

    }
}

