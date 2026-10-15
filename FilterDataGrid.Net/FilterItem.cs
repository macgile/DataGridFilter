#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterItem.cs
// Created    : 26/01/2021

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterDataGrid
{
    public abstract class FilterBase : NotifyProperty
    {
        #region Public Properties

        /// <summary>
        ///     Raw value of the item (not displayed, see Label property)
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        ///     Content length
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        ///     Field type
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Type FieldType { get; set; }

        /// <summary>
        ///     State change flag
        /// </summary>
        public bool IsChanged { get; set; }

        /// <summary>
        ///     Content displayed
        /// </summary>
        public object Label { get; set; }

        /// <summary>
        ///     Hierarchical level
        /// </summary>
        public int Level { get; set; }

        #endregion Public Properties
    }

    public class FilterItem : FilterBase
    {
        #region Private Fields

        private bool initialState;
        private bool isChecked;

        #endregion Private Fields

        #region Public Properties

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

        #endregion Public Properties
    }

    public class FilterItemDate : FilterBase
    {
        #region Private Fields

        private bool? initialState;
        private bool? isChecked;

        #endregion Private Fields

        #region Public Properties

        public List<FilterItemDate> Children { get; set; }

        /// <summary>
        /// Initial state
        /// </summary>
        public bool? Initialize
        {
            set
            {
                initialState = value;
                isChecked = value;
            }
        }

        /// <summary>
        /// State of checkbox
        /// </summary>
        public bool? IsChecked
        {
            get => isChecked;
            set => SetIsChecked(value, true, true);
        }

        public FilterItem Item { get; set; }
        public FilterItemDate Parent { get; set; }
        public List<FilterItemDate> Tree { get; set; }

        #endregion Public Properties

        #region Private Methods

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked) return;

            isChecked = value;

            IsChanged = initialState != isChecked;

            // filter Item linked to the day, it propagates the states changes.
            // Only the days have a reference to an item in the list used to generate the tree.
            if (Item != null)
            {
                Item.IsChanged = IsChanged;
                Item.Initialize = IsChecked == true;
            }

            // (Select All) item
            if (Level == 0)
            {
                List<FilterItemDate> itemsToUpdate = Tree?.Skip(1).ToList();
                if (itemsToUpdate != null)
                {
                    foreach (FilterItemDate item in itemsToUpdate)
                    {
                        item.SetIsChecked(value, true, true);
                    }
                }
            }

            // state.HasValue : !null
            if (updateChildren && isChecked.HasValue && Level > 0)
            {
                if (Children != null)
                {
                    foreach (FilterItemDate child in Children)
                    {
                        child.SetIsChecked(value, true, false);
                    }
                }
            }

            if (updateParent) Parent?.VerifyCheckedState();

            OnPropertyChanged(nameof(IsChecked));
        }

        private void VerifyCheckedState()
        {
            bool? b = null;

            for (int i = 0; i < Children.Count; ++i)
            {
                FilterItemDate item = Children[i];
                bool? current = item.IsChecked;

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