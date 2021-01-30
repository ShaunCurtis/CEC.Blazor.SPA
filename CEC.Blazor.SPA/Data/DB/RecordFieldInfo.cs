/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using CEC.Blazor.Extensions;
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

        public string DisplayName { get; set; }

        public RecordFieldInfo(string name, string displayname, string guidString)
        {
            FieldName = name;
            DisplayName = displayname;
            if (Guid.TryParse(guidString, out Guid guid)) this.FieldGUID = guid;
            else this.FieldGUID = Guid.NewGuid();
        }

        public RecordFieldInfo(string name, string displayname, Guid guid)
        {
            FieldName = name;
            DisplayName = displayname;
            this.FieldGUID = guid;
        }

        public RecordFieldInfo(string name, Guid guid)
        {
            FieldName = name;
            DisplayName = name.AsSeparatedString();
            this.FieldGUID = guid;
        }

        public RecordFieldInfo(string name, string displayname)
        {
            FieldName = name;
            DisplayName = displayname;
            this.FieldGUID = Guid.NewGuid(); ;
        }

        public RecordFieldInfo(string name)
        {
            FieldName = name;
            DisplayName = name.AsSeparatedString();
            this.FieldGUID = Guid.NewGuid(); ;
        }
    }
}
