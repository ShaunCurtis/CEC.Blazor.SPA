
using System;

namespace CEC.Blazor.Data
{
    public class RecordValue
    {
        public string Field { get; }

        public Guid GUID { get; }

        public object Value { get; }

        public object EditedValue { get; set; }

        public bool IsDirty
        {
            get
            {
                if (Value != null && EditedValue != null) return !Value.Equals(EditedValue);
                if (Value is null && EditedValue is null) return false;
                return true;
            }
        }

        public RecordValue(string field, object value)
        {
            this.Field = field;
            this.Value = value;
            this.EditedValue = value;
            this.GUID = Guid.NewGuid();
        }

        public RecordValue(RecordFieldInfo field, object value)
        {
            this.Field = field.FieldName;
            this.Value = value;
            this.EditedValue = value;
            this.GUID = field.FieldGUID;
        }

        public void Reset()
            => this.EditedValue = this.Value;
    }
}
