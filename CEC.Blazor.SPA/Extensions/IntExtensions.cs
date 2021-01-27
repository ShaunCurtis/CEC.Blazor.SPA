/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Globalization;

namespace CEC.Blazor.Extensions
{
    public static class IntExtensions
    {

        public static string CheckforNull(this int value) => value == int.MinValue ? "No Value" : value.ToString();

        public static int AsInt(this int? value) => value is null ? 0 : (int)value;

        public static string AsMonthName(this int value) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(value);


    }
}
