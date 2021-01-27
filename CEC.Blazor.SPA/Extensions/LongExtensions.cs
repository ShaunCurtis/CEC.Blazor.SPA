/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Globalization;

namespace CEC.Blazor.Extensions
{
    public static class LongExtensions
    {

        public static long AsLong(this long? value) => value is null ? 0 : (long)value;

    }
}
