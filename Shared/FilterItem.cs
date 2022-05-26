#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterItem.cs
// Created    : 26/01/2021


#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace FilterDataGrid
{
    public interface IFilter
    {
        #region Public Properties

        object Content { get; set; }
        int ContentLength { get; set; }
        Type FieldType { get; set; }
        bool IsChanged { get; set; }
        string Label { get; set; }
        int Level { get; set; }
        #endregion Public Properties
    }

    public class FilterItem : NotifyProperty, IFilter
    {
        #region Private Fields

        private bool initialState;
        private bool isChecked;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///     Raw value of the item (not displayed, see Label property)
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Content length
        /// </summary>
        public int ContentLength { get; set; }


        /// <summary>
        ///     Field type
        /// </summary>
        public Type FieldType { get; set; }

        /// <summary>
        /// Initial state
        /// </summary>
        public bool Initialize
        {
            set
            {
                initialState = value;
                isChecked = value;
            }
        }

        public bool IsChanged { get; set; }

        /// <summary>
        /// State of checkbox
        /// </summary>
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                IsChanged = value != initialState;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>
        /// Content displayed
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Hierarchical level for the date
        /// </summary>
        public int Level { get; set; }

        #endregion Public Properties
    }

    public class FilterItemDate : NotifyProperty, IFilter
    {
        #region Private Fields

        private bool? initialState;
        private bool? isChecked;

        #endregion Private Fields

        #region Public Properties

        public List<FilterItemDate> Children { get; set; }

        public object Content { get; set; }

        public int ContentLength { get; set; }

        public Type FieldType { get; set; }

        public bool? Initialize
        {
            set
            {
                initialState = value;
                isChecked = value;
            }
        }

        public bool IsChanged { get; set; }

        public bool? IsChecked
        {
            get => isChecked;
            set => SetIsChecked(value, true, true);
        }

        public FilterItem Item { get; set; }

        public string Label { get; set; }

        public int Level { get; set; }

        public FilterItemDate Parent { get; set; }

        public List<FilterItemDate> Tree { get; set; }

        #endregion Public Properties

        #region Private Methods

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked) return;

            isChecked = value;

            IsChanged = initialState != isChecked;

            // filter Item linked to the day, it propagates the status changes
            if (Item != null)
            {
                Item.IsChanged = IsChanged;
                Item.Initialize = IsChecked == true;
            }

            // (Select All) item
            if (Level == 0)
                Tree?.Skip(1).ToList().ForEach(c => { c.SetIsChecked(value, true, true); });

            // state.HasValue : !null
            if (updateChildren && isChecked.HasValue && Level > 0)
                Children?.ForEach(c => { c.SetIsChecked(value, true, false); });

            if (updateParent) Parent?.VerifyCheckedState();

            OnPropertyChanged(nameof(IsChecked));
        }

        private void VerifyCheckedState()
        {
            bool? b = null;

            for (var i = 0; i < Children.Count; ++i)
            {
                var item = Children[i];
                var current = item.IsChecked;

                if (i == 0)
                {
                    b = current;
                }
                else if (b != current)
                {
                    b = null;
                    break;
                }
            }

            SetIsChecked(b, false, true);
        }

        #endregion Private Methods
    }
}