/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================
using System.Globalization;

namespace CEC.Blazor.Extensions
{
    public static class DecimalExtensions
    {

        public static string DecimalPlaces(this decimal value, int places ) => value.ToString($"N{places}", CultureInfo.CurrentCulture);

        public static string AsPercentage(this decimal value) => string.Format("{0}%", value.ToString("#0.##"));

        public static string AsSterling(this decimal value) => string.Format("£{0}", value.ToString("#,##0.00"));

        public static string CheckforNull(this decimal value) => value == decimal.MinValue ? "No Value" : value.ToString();


    }
}
