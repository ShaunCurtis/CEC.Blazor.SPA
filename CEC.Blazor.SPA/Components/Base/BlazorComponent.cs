/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace CEC.Blazor.SPA
{
    /// <summary>
    /// Abstract Base Class implementing OnAfterRender
    /// Much of the code is common with ComponentBase
    /// Blazor Team copyright notices recognised.
    /// </summary>
    public abstract class BlazorComponent : BaseBlazorComponent, IComponent, IHandleEvent, IHandleAfterRender
    {
        // Bool global to track the after render process 
        private bool _hasCalledOnAfterRender;

        /// <summary>
        /// Internal method to track the render event
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object arg)
        {
            var task = callback.InvokeAsync(arg);
            var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled;
            InvokeAsync(Render);
            return shouldAwaitTask ? CallRenderOnAsyncCompletion(task) : Task.CompletedTask;
        }

        /// <summary>
        /// Internal Method triggered after the component has rendered calling OnAfterRenderAsync
        /// </summary>
        /// <returns></returns>
        Task IHandleAfterRender.OnAfterRenderAsync()
        {
            var firstRender = !_hasCalledOnAfterRender;
            _hasCalledOnAfterRender |= true;
            return OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Internal method to handle render completion
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task CallRenderOnAsyncCompletion(Task task)
        {
            try { await task; }
            catch
            {
                if (task.IsCanceled) return;
                else throw;
            }
            await InvokeAsync(Render);
        }
    }
}
