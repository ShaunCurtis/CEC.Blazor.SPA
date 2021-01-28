/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================
using CEC.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCECBlazorSPA(this IServiceCollection services)
        {
            services.AddScoped<BrowserService>();
            return services;
        }
    }
}
