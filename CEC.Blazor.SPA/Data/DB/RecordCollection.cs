﻿/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Clear() => _items.Clear();

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

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(_items);
        }

        public T Get<T>(string FieldName) 
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.Value is T t) return t;
            return default;
        }

        public T GetEditValue<T>(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.EditedValue is T t) return t;
            return default;
        }

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

        public bool TryGet<T>(string FieldName, out T value)
        {
            value = default;
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.Value is T t) value = t;
            return x.Value != default;
        }

        public bool TryGetEditValue<T>(string FieldName, out T value)
        {
            value = default;
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x != null && x.EditedValue is T t) value = t;
            return x.EditedValue != default;
        }

        public bool HasField(string FieldName)
        {
            var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
            if (x is null | x == default) return false;
            return true;
        }

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

    public class Enumerator : IEnumerator
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
        public Enumerator(List<RecordValue> items)
        {
            this._items = items;
            _cursor = -1;
        }
        void IEnumerator.Reset()
        {
            _cursor = -1;
        }
        bool IEnumerator.MoveNext()
        {
            if (_cursor < _items.Count)
                _cursor++;

            return (!(_cursor == _items.Count));
        }

    }
}