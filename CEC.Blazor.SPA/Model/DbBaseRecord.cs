
using System;
/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================
namespace CEC.Blazor.Data
{
    /// <summary>
    /// Base Data Record
    /// </summary>
    public class DbBaseRecord :IDbRecord<DbBaseRecord>
    {

        public int ID { get; set; } = -1;

        public Guid GUID => Guid.Empty;

        public string DisplayName { get; set; }

        public DbRecordInfo RecordInfo => new DbRecordInfo();

        public RecordCollection AsProperties() =>
            new RecordCollection()
            {
                { "ID", this.ID },
                { "DisplayName", this.DisplayName }
        };

        public static DbBaseRecord FromProperties(RecordCollection props) =>
            new DbBaseRecord()
            {
                ID = props.Get<int>("ID"),
                DisplayName = props.Get<string>("DisplayName")
            };

        public DbBaseRecord GetFromProperties(RecordCollection props) => DbBaseRecord.FromProperties(props);

        public DbBaseRecord ShadowCopy()
        {
            return new DbBaseRecord() {
                ID = this.ID,
                DisplayName = this.DisplayName
            };
        }
    }
}
