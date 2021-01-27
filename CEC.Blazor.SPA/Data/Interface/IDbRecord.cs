/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Common Interface definition for any DbRecord
    /// </summary>
    public interface IDbRecord<TRecord> where TRecord : class, IDbRecord<TRecord>, new()
    {
        /// <summary>
        /// ID to ensure we have a unique key
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// ID to ensure we have a unique key
        /// </summary>
        public Guid GUID { get; }

        /// <summary>
        /// Display name for the Record
        /// Point to the field that you want to use for the dipslay name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Record Information used by the Data Services
        /// </summary>
        public DbRecordInfo RecordInfo { get; }

        /// <summary>
        /// Gets a Record Collection of the Record Values
        /// Used by Editor to track state
        /// </summary>
        /// <returns></returns>
        public RecordCollection AsProperties();

        /// <summary>
        /// Static Method to get a new Record from a Property collection
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static TRecord FromProperties(RecordCollection props) => default;

        /// <summary>
        /// Static Method to get a new Record from a Property collection
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public TRecord GetFromProperties(RecordCollection props);

        /// <summary>
        /// Method to get the defined fields for running CUD Stored Procedures on the Record
        /// </summary>
        /// <returns></returns>
        public List<PropertyInfo> GetSPParameters()
        {
            var attrprops = new List<PropertyInfo>();
            foreach (var prop in typeof(TRecord).GetProperties())
            {
                if (HasSPParameterAttribute(prop.Name)) attrprops.Add(prop);
            }
            return attrprops;
        }

        /// <summary>
        /// Method to Check if a property has SPParameter Attribute
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static bool HasSPParameterAttribute(string propertyName)
        {
            var prop = typeof(TRecord).GetProperty(propertyName);
            var attrs = prop.GetCustomAttributes(true);
            var attribute = (SPParameterAttribute)attrs.FirstOrDefault(item => item.GetType() == typeof(SPParameterAttribute));
            return attribute != null;
        }

    }
}
