/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Collections.Generic;
using System.Reflection;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class to hold Database Access Information for a IDbRecord
    /// </summary>
    public class DbRecordInfo
    {
        /// <summary>
        /// Create Stored Procedure
        /// </summary>
        public string CreateSP { get; init; }

        /// <summary>
        /// Update Stored Procedure
        /// </summary>
        public string UpdateSP { get; init; }

        /// <summary>
        /// Delete Stored Procedure
        /// </summary>
        public string DeleteSP { get; init; }

        public string RecordName { get; init; } = "Record";

        public string RecordListName { get; init; } = "Records";

        public string RecordDescription { get; init; } = "Record";

        public string RecordListDescription { get; init; } = "Records";

        /// <summary>
        /// List of all the Properties with the SPParameter Attribute
        /// </summary>
        public List<PropertyInfo> SPProperties { get; set; } = new List<PropertyInfo>();

    }
}
