#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterItem.cs
// Created    : 26/01/2021
// 

#endregion

using System;
using System.Collections.Generic;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FilterDataGrid
{
    public class FilterItem : NotifyProperty
    {
        #region Public Events

        public event EventHandler<bool?> OnIsCheckedDate;

        #endregion Public Events

        #region Private Fields

        private bool isChecked;
        private bool? isDateChecked;
        private bool notify;

        #endregion Private Fields

        #region Public Properties

        public List<FilterItem> Children { get; set; }

        // raw value of the item (not displayed, see Label property)
        public object Content { get; set; }

        public FilterCommon CurrentFilter { get; set; }

        public Type FieldType { get; set; }

        public int Id { get; set; }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (value == isChecked) return;
                isChecked = value;

                if (notify)
                    OnPropertyChanged("IsChecked");

                // reactivate notify
                // the iteration over an Collection triggers the notification
                // of the "IsChecked" property and slows the performance of the loop
                notify = true;
            }
        }

        public bool? IsDateChecked
        {
            get => isDateChecked;
            set
            {
                // raise event to update the date tree
                // see FilterCommon class
                OnIsCheckedDate?.Invoke(this, value);
            }
        }

        // content displayed
        public string Label { get; set; }

        public int Level { get; set; }

        public FilterItem Parent { get; set; }

        // don't invoke update tree
        public bool? SetDateState
        {
            get => isDateChecked;
            set => isDateChecked = value;
        }

        #endregion Public Properties
    }
}
