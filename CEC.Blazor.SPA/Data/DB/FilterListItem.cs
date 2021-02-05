using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEC.Blazor.Data
{
    public class FilterListItem
    {
        public string FieldName { get; set; }

        public object Value
        {
            get => _Value;
            set
            {
                _Value = value;
            }
        }

        private object _Value = null;

        public Type ObjectType { get; set; }

    }
}
