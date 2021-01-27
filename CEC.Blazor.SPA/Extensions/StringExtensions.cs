/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Text.RegularExpressions;

namespace CEC.Blazor.Extensions
{
    public static class StringExtensions
    {
        public static string AsSizedString(this string value, bool dotting, int size = 50)
        {
            if (value != null)
            {
                if (value.Length > size - 3 && dotting) return string.Concat(value.Substring(0, size - 3), "...");
                else if (value.Length > size) return value.Substring(0, size);
                else return value;
            }
            return string.Empty;
        }

        public static string AsSeparatedString(this string value) => string.IsNullOrEmpty(value) ? string.Empty : Regex.Replace(value, @"\B[A-Z]", " $0");

        public static string TextToHtml(this string text) => string.IsNullOrEmpty(text) ? string.Empty : text.Replace(System.Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");

        public static string AsHtml(this string text) => string.IsNullOrEmpty(text) ? string.Empty : text.Replace(System.Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");

        public static bool IsEmpty(this string value) => (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));

        public static bool IsNotEmpty(this string value) => !(string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));

    }
}
