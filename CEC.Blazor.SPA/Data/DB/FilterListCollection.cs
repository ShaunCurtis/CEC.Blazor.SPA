/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Collections.Generic;
using System.Linq;

namespace CEC.Blazor.Data
{

    public enum FilterListCollectionState
    {
        NotSet = 0,
        Show = 1,
        Hide = 2
    }

    public class FilterListCollection :IEnumerable<FilterListItem>
    {
        private List<FilterListItem> filterList = new List<FilterListItem>();

        public FilterListCollectionState ShowState { get; set; } = FilterListCollectionState.NotSet;

        public bool OnlyLoadIfFilters { get; set; } = false;

        /// <summary>
        /// Boolean to determine if the filter should be shown in the UI
        /// </summary>
        public bool Show => this.ShowState == FilterListCollectionState.Show;

        /// <summary>
        /// Boolean to tell the list loader if it need to load
        /// </summary>
        public bool Load => this.filterList.Count > 0 || !this.OnlyLoadIfFilters;

        public IEnumerator<FilterListItem> GetEnumerator()
        {
            foreach (var item in filterList)
                yield return item;
        }

    public bool TryGetFilter(string name, out object value)
        {
            value = null;
            var filter = filterList.FirstOrDefault(FilterListItem => FilterListItem.FieldName.Equals(name));
            if (filter != default)
            {
                value = filter.Value;
                return true;
            }
            return false;
        }

        public bool SetFilter(string name, object value, bool overwite = true)
        {
            var filter = filterList.FirstOrDefault(FilterListItem => FilterListItem.FieldName.Equals(name));
            if (filter == default)
            {
                filterList.Add(new FilterListItem() { FieldName = name, Value = value, ObjectType = value.GetType()});
                return true;
            }
            else
            {
                if (overwite)
                {
                    filter.Value = value;
                    filter.ObjectType = value.GetType();
                    return true;
                }
                return false;
            }
        }

        public bool ClearFilter(string name)
        {
            var filter = filterList.FirstOrDefault(FilterListItem => FilterListItem.FieldName.Equals(name));
            if (filter != default)
            {
                filterList.Remove(filter);
                return true;
            }
            return false;
        }


    }
}
