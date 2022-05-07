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

namespace FilterDataGrid
{
    public interface IFilter
    {
        #region Public Properties

        object Content { get; set; }
        Type FieldType { get; set; }
        int[] GroupIndex { get; set; }
        int Index { get; set; }
        bool IsChanged { get; set; }
        bool IsPrevious { get; set; }
        int Level { get; set; }
        bool State { get; set; }

        #endregion Public Properties
    }

    public struct GroupIndexState
    {
        #region Public Properties

        public List<int> CheckedIndex { get; set; }
        public object Content { get; set; }
        public bool IsChecked { get; set; }
        public bool IsPrevious { get; set; }
        public List<int> PreviousIndex { get; set; }
        public bool IsNull { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    ///     ListBox Item
    /// </summary>
    public class FilterItem : Notify, IFilter, IDisposable
    {
        #region Private Fields

        private bool initialState;

        private bool isChecked;

        #endregion Private Fields

        #region Public Constructors

        public FilterItem(EventHandler<bool?> selectAll = null)
        {
            if (selectAll != null)
                SelectAll += selectAll;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler<bool?> EventSelectAll
        {
            add => SelectAll += value;
            remove => SelectAll -= value;
        }

        public event EventHandler<bool?> SelectAll;

        #endregion Public Events

        #region Public Properties

        public object Content { get; set; }

        public int ContentLength => Content?.ToString().Length ?? 0;

        public Type FieldType { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool FromSelectAll { get; set; }

        public int[] GroupIndex { get; set; }

        public int Index { get; set; }

        public bool Initialize
        {
            get => initialState;
            set
            {
                initialState = value;
                isChecked = value;
            }
        }

        // isChecked != initialState;
        public bool IsChanged { get; set; }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                IsChanged = value != initialState;
                OnPropertyChanged(nameof(IsChecked));

                // Debug.WriteLine($"Level : {Level, -4}Content :{Content}");

                // select all
                if (Level == 0) OnSelectAll();
            }
        }

        public bool IsPrevious { get; set; }

        public int Level { get; set; }

        public bool State { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            SelectAll = null;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnSelectAll()
        {
            SelectAll?.Invoke(this, isChecked);
        }

        #endregion Private Methods
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

        public int Index { get; set; }

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

        public string Label { get; set; }

        public int Level { get; set; }

        public FilterItemDate Parent { get; set; }

        public FilterItem Item { get; set; }
        
        public bool State
        {
            get => isChecked == true;
            set => isChecked = value;
        }

        public List<FilterItemDate> Tree { get; set; }

        #endregion Public Properties

        #region Private Methods

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked) return;

            isChecked = value;

            // triggers nothing!
            IsChanged = initialState != isChecked;

            if (Item != null)
            {
                Item.IsChanged = IsChanged;
                Item.Initialize = IsChecked == true;
            }
            
            if(Level == 0)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods
    }
}