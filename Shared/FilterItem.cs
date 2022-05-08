// Author     : Gilles Macabies
// Solution   : FIlterConsole
// Projet     : FilterDataGrid
// File       : FilterItem.cs
// Created    : 02/05/2022
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMemberInSuper.Global

namespace FilterDataGrid
{
    public interface IFilter
    {
        #region Public Properties

        object Content { get; set; }
        Type FieldType { get; set; }
        int[] GroupIndex { get; set; }
        bool IsChanged { get; set; }
        bool IsPrevious { get; set; }
        int Level { get; set; }

        #endregion Public Properties
    }

    public struct GroupIndexState
    {
        #region Public Properties

        public List<int> CheckedIndex { get; set; }
        public object Content { get; set; }
        public bool IsChecked { get; set; }
        public bool IsNull { get; set; }
        public bool IsPrevious { get; set; }
        public List<int> PreviousIndex { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    ///     ListBox Item
    /// </summary>
    public class FilterItem : Notify, IFilter
    {
        #region Private Fields

        private bool initialState;

        private bool isChecked;

        #endregion Private Fields

        #region Public Properties

        public object Content { get; set; }

        public int ContentLength => Content?.ToString().Length ?? 0;

        public Type FieldType { get; set; }

        public int[] GroupIndex { get; set; }

        public bool Initialize
        {
            set
            {
                initialState = value;
                isChecked = value;
            }
        }

        public bool IsChanged { get; set; }

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

        public bool IsPrevious { get; set; }

        public int Level { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    ///     TreeView Item
    /// </summary>
    public class FilterItemDate : Notify, IFilter
    {
        #region Private Fields

        private bool? initialState;
        private bool? isChecked;

        #endregion Private Fields

        #region Public Properties

        public List<FilterItemDate> Children { get; set; }

        public object Content { get; set; }

        public Type FieldType { get; set; }

        public int[] GroupIndex { get; set; }

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

        public bool IsPrevious { get; set; }

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

            if (Item != null)
            {
                Item.IsChanged = IsChanged;
                Item.Initialize = IsChecked == true;
            }

            if (Level == 0)
                Tree?.Skip(1).ToList().ForEach(c => { c.SetIsChecked(value, true, true); });

            // state.HasValue : !null
            if (updateChildren && isChecked.HasValue && Level != -1)
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

    public class Notify : INotifyPropertyChanged
    {
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Protected Methods

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods
    }
}