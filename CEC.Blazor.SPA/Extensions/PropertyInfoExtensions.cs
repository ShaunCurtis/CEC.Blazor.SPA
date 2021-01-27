using System.Reflection;

namespace CEC.Blazor.Extensions
{
    /// <summary>
    /// Method to get a PropertyInfo object as a string
    /// </summary>
    public static class PropertyInfoExtensions
    {
        public static string GetValueAsString(this PropertyInfo value, object obj) =>
            value?.GetValue(obj)?.ToString() ?? string.Empty;
    }
}
