using System;
using System.Collections.Generic;
using CEC.Blazor.Extensions;

namespace CEC.Blazor.Utilities
{
    public class Utils
    {

        /// <summary>
        /// Method to get a sorted deictionary list of an enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static SortedDictionary<int, string> GetEnumList<TEnum>()
        {
            var list = new SortedDictionary<int, string>();
            if (typeof(TEnum).IsEnum)
            {
                var values = Enum.GetValues(typeof(TEnum));
                foreach (int v in values) list.Add(v, (Enum.GetName(typeof(TEnum), v)).AsSeparatedString());
            }
            return list;
        }
    }
}
