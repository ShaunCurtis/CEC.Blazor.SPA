﻿/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

namespace CEC.Blazor.Extensions
{
    public static class boolExtensions
    {
        public static string AsYesNo(this bool value) => value ? "Yes" : "No";

    }
}
