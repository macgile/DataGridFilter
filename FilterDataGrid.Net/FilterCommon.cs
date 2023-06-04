#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterCommon.cs
// Created    : 26/01/2021
//

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FilterDataGrid
{
    public sealed class FilterCommon : NotifyProperty, ISerializable
    {
        #region Private Fields

        private bool isFiltered;

        #endregion Private Fields

        #region Public Constructors

        public FilterCommon()
        {
            FilteredItems = new HashSet<object>(EqualityComparer<object>.Default);
            Criteria = new HashSet<Predicate<object>>();
        }

        protected FilterCommon(SerializationInfo info, StreamingContext context)
        {
            FieldName = info.GetString(nameof(FieldName));
            FilteredItems = new HashSet<object>((object[])info.GetValue(nameof(FilteredItems), typeof(object[])));
            IsFiltered = info.GetBoolean(nameof(IsFiltered));
        }

        #endregion Public Constructors

        #region Public Properties

        public string FieldName { get; set; }

        public Type FieldType { get; set; }

        public PropertyInfo FieldProperty { get; set; }

        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                isFiltered = value;
                OnPropertyChanged("IsFiltered");
            }
        }

        public HashSet<object> FilteredItems { get; set; }

        public HashSet<Predicate<object>> Criteria { get; set; }

        public Loc Translate { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///     Add the filter to the predicate dictionary
        /// </summary>
        public void AddCriteria()
        {
            if (IsFiltered) return;

            // predicate of filter
            bool Predicate(object o)
            {
                var value = FieldType == typeof(DateTime)
                    ? ((DateTime?)FieldProperty?.GetValue(o, null))?.Date
                    : FieldProperty?.GetValue(o, null);

                return FilteredItems.Contains(value);
            }

            // add to list of predicates
            Criteria.Add(Predicate);
            IsFiltered = true;
        }

        public bool RemoveFilter()
        {
            if (!IsFiltered) return false;

            Criteria.Clear();
            isFiltered = false;
            return true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(FieldName), FieldName);
            info.AddValue(nameof(FilteredItems), FilteredItems);
            info.AddValue(nameof(IsFiltered), IsFiltered);
        }

        #endregion Public Methods
    }
}