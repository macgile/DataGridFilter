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
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FilterDataGrid
{
    public sealed class FilterCommon : NotifyProperty
    {
        #region Private Fields

        private bool isFiltered;

        #endregion Private Fields

        #region Public Constructors

        public FilterCommon()
        {
            PreviouslyFilteredItems = new HashSet<object>(EqualityComparer<object>.Default);
            Criteria = new HashSet<Predicate<object>>();
        }

        #endregion Public Constructors

        #region Public Properties

        public string FieldName { get; set; }

        [JsonIgnore]
        public Type FieldType { get; set; }

        [JsonIgnore]
        public PropertyInfo FieldProperty { get; set; }

        [JsonIgnore]
        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                isFiltered = value;
                OnPropertyChanged("IsFiltered");
            }
        }

        public HashSet<object> PreviouslyFilteredItems { get; set; }

        [JsonIgnore]
        public HashSet<Predicate<object>> Criteria { get; set; }

        [JsonIgnore]
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

                return !PreviouslyFilteredItems.Contains(value);
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

        #endregion Public Methods
    }
}