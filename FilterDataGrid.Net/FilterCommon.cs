﻿#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterCommon.cs
// Created    : 26/01/2021
//

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FilterDataGrid
{
    [DataContract]
    public sealed class FilterCommon : NotifyProperty
    {
        #region Private Fields

        private bool isFiltered;

        #endregion Private Fields

        #region Public Properties

        public HashSet<object> PreviouslyFilteredItems { get; set; } = new HashSet<object>(EqualityComparer<object>.Default);

        [DataMember(Name = "FilteredItems")]
        public List<object> FilteredItems
        {
            get
            {
                return FieldType?.BaseType == typeof(Enum)
                    ? PreviouslyFilteredItems.ToList().ConvertAll(f => (object)f.ToString())
                    : PreviouslyFilteredItems?.ToList();
            }

            set => PreviouslyFilteredItems = value.ToHashSet();
        }

        [DataMember(Name = "FieldName")]
        public string FieldName { get; set; }

        public Button FilterButton { get; set; }
        public Loc Translate { get; set; }
        public Type FieldType { get; set; }

        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                isFiltered = value;
                OnPropertyChanged(nameof(IsFiltered));
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///     Add the filter to the predicate dictionary
        /// </summary>
        public void AddFilter(Dictionary<string, Predicate<object>> criteria)
        {
            if (IsFiltered) return;

            // add to list of predicates
            criteria.Add(FieldName, Predicate);

            IsFiltered = true;
            return;

            // predicate of filter
            bool Predicate(object o)
            {
                var value = FieldType == typeof(DateTime)
                    ? ((DateTime?)o.GetPropertyValue(FieldName))?.Date
                    : o.GetPropertyValue(FieldName);

                return !PreviouslyFilteredItems.Contains(value);
            }
        }

        #endregion Public Methods
    }
}