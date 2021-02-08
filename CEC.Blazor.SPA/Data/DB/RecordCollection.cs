/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CEC.Blazor.Data
{
    public class RecordCollection : ICollection
    {
        private List<RecordValue> _items = new List<RecordValue>() ;

        public int Count => _items.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public Action<bool> FieldValueChanged;

        public bool IsDirty => _items.Any(item => item.IsDirty);

        public void Clear() 
            => _items.Clear();

        public void CopyTo(Array array, int index)
        {
            foreach (var i in array)
            {
                if (i is RecordValue value) this.Add(value);
            }
        }

        public void AddCollection(RecordCollection collection)
        {
            if (collection != null)
            {
                foreach (var i in collection)
                {
                    if (i is RecordValue value) this.Add(value);
                }
            }
        }

        public void ResetValues()
            => _items.ForEach(item => item.Reset());

        public IEnumerator GetEnumerator()
        {
            return new RecordCollectionEnumerator(_items);
        }
        public T Get<T>(RecordFieldInfo field) 
            => Get<T>(field.FieldName);

        public T Get<T>(string FieldName) 
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.Value is T t) return t;
            return default;
        }

        public T GetEditValue<T>(RecordFieldInfo field) 
            => GetEditValue<T>(field.FieldName);

        public T GetEditValue<T>(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.EditedValue is T t) return t;
            return default;
        }

        public RecordValue GetRecordValue(RecordFieldInfo field) 
            => GetRecordValue(field.FieldName);

        public RecordValue GetRecordValue(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x == default)
            {
                x = new RecordValue(FieldName, null);
                _items.Add(x);
            }
            return x;
        }

        public bool TryGet<T>(RecordFieldInfo field, out T value) 
            => this.TryGet<T>(field.FieldName, out value);

        public bool TryGet<T>(string FieldName, out T value)
        {
            value = default;
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.Value is T t) value = t;
            return x.Value != default;
        }

        public bool TryGetEditValue<T>(RecordFieldInfo field, out T value) 
            => this.TryGetEditValue<T>(field.FieldName, out value);

        public bool TryGetEditValue<T>(string FieldName, out T value)
        {
            value = default;
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.EditedValue is T t) value = t;
            return x.EditedValue != default;
        }

        public bool HasField(RecordFieldInfo field) 
            => this.HasField(field.FieldName);

        public bool HasField(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x is null | x == default) return false;
            return true;
        }

        public bool SetField(RecordFieldInfo field, object value) 
            => this.SetField(field.FieldName, value);

        public bool SetField(string FieldName, object value )
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x != default)
            {
                x.EditedValue = value;
                this.FieldValueChanged?.Invoke(this.IsDirty);
            }
            else _items.Add(new RecordValue(FieldName, value));
            return true;
        }

        public bool Add(RecordFieldInfo field, object value)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(field.FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x != default) _items.Remove(x);
            _items.Add(new RecordValue(field, value));
            return true;
        }

        public bool Add(string FieldName, object value)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x != default) _items.Remove(x);
            _items.Add(new RecordValue(FieldName, value));
            return true;
        }

        public bool Add(RecordValue value)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(value.Field, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x != default) _items.Remove(x);
            _items.Add(value);
            return true;
        }

        public bool RemoveField(RecordFieldInfo field) 
            => RemoveField(field.FieldName);

        public bool RemoveField(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x != default)
            {
                _items.Remove(x);
                return true;
            }
            return false;
        }
    }

    public class RecordCollectionEnumerator : IEnumerator
    {
        private List<RecordValue> _items = new List<RecordValue>();
        private int _cursor;

        object IEnumerator.Current
        {
            get
            {
                if ((_cursor < 0) || (_cursor == _items.Count))
                    throw new InvalidOperationException();
                return _items[_cursor];
            }
        }
        public RecordCollectionEnumerator(List<RecordValue> items)
        {
            this._items = items;
            _cursor = -1;
        }
        void IEnumerator.Reset()
            => _cursor = -1;

        bool IEnumerator.MoveNext()
        {
            if (_cursor < _items.Count)
                _cursor++;

            return (!(_cursor == _items.Count));
        }
    }
}
