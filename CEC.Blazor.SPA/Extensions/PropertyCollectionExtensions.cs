/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================
using System.Data;

namespace CEC.Blazor.Extensions
{
    public class PropertyConstants
    {
        public static readonly string MaxColumn = "MaxColumn";
        public static readonly string UseModalViewer = "UseModalViewer";
        public static readonly string RowNavigateToViewer = "RowNavigateToViewer";
        public static readonly string ShowButtons = "ShowButtons";
        public static readonly string ShowAdd = "ShowAdd";
        public static readonly string ShowEdit = "ShowEdit";
        public static readonly string MaxColumnPercent = "MaxColumnPercent";
        //public static readonly string m = "";
    }

    public static class PropertyCollectionExtensions
    {
        /// <summary>
        /// Generic method to get a property value.  Will return default T if null or return value os not T 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(this PropertyCollection collection, string name)
        {
            if (collection.ContainsKey(name))
            {
                var value = collection[name];
                if (value is T t) return t;
            }
            return default;
        }
        /// <summary>
        /// Generic method to get a property value.  Will set value will be default T if null or return value os not T 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGet<T>(this PropertyCollection collection, string name, out T value)
        {
            value = default;

            if (collection.ContainsKey(name))
            {
                var val = collection[name];
                if (val.GetType() is T)
                {
                    value = (T)val;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method to get a Property value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGet(this PropertyCollection collection, string name, out object value)
        {
            value = null;
            if (collection.ContainsKey(name)) value = collection[name];
            return value != null;
        }

        /// <summary>
        /// Method to set a property
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Set(this PropertyCollection collection, string name, object value)
        {
            if (collection.ContainsKey(name)) collection[name] = value;
            else collection.Add(name, value);
            return collection.ContainsKey(name);
        }

        /// <summary>
        /// Method to clear a property out of the property list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Clear(this PropertyCollection collection, string name)
        {
            if (collection.ContainsKey(name)) collection.Remove(name);
            return !collection.ContainsKey(name);
        }

    }
}
