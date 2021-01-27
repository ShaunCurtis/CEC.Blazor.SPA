/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class to hold Database Access Information for a IDbRecord
    /// </summary>
    public class DbDistinctRequest
    {
        public string FieldName { get; set; }

         public string DistinctSetName { get; set; }

        public string QuerySetName { get; set; }
    }
}
