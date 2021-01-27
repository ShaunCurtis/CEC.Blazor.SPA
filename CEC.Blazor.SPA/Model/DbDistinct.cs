/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.ComponentModel.DataAnnotations;

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Base Data Record for Distint Records
    /// </summary>
    public class DbDistinct : IDbRecord<DbDistinct>
    {
        [Key]
        public string Value { get; set; }

        public int ID => 0;

        public Guid GUID => Guid.Empty;

        public string DisplayName => Value;

        public DbRecordInfo RecordInfo => new DbRecordInfo();

        public RecordCollection AsProperties()
        {
            throw new System.NotImplementedException();
        }

        public DbDistinct GetFromProperties(RecordCollection props)
        {
            throw new System.NotImplementedException();
        }

        public DbDistinct ShadowCopy()
        {
            throw new System.NotImplementedException();
        }
    }
}
