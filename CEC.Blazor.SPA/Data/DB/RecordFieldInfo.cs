/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// record to hold Record Field Information
    /// </summary>
    public class RecordFieldInfo
    {
        public string FieldName { get; init; }

        public Guid FieldGUID { get; init; }

        public RecordFieldInfo(string name, string guidString)
        {
            FieldName = name;
            if (Guid.TryParse(guidString, out Guid guid)) this.FieldGUID = guid;
            else this.FieldGUID = Guid.NewGuid();
        }
        public RecordFieldInfo(string name, Guid guid)
        {
            FieldName = name;
            this.FieldGUID = guid;
        }

        public RecordFieldInfo(string name)
        {
            FieldName = name;
            this.FieldGUID = Guid.NewGuid(); ;
        }

    }
}
