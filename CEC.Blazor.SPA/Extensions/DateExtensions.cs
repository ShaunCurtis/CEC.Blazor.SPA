/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Globalization;

namespace CEC.Blazor.Extensions
{
    public static class DateExtensions
    {
        public static string AsShortDate(this DateTime value) => value.ToString("dd-MMM-yyyy");

        public static string AsVeryShortDate(this DateTime value) => value.ToString("dd-MM-yy");

        public static string AsShortDateTime(this DateTime value) => value.ToString("h:mm tt dd-MMM-yyyy");

        public static string AsMonthYear(this DateTime value) => value.ToString("MMM-yyyy");

    }
}
