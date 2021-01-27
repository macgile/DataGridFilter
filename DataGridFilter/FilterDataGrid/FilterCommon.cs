#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterCommon.cs
// Created    : 26/01/2021
// 

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable InvalidXmlDocComment
// ReSharper disable TooManyChainedReferences
// ReSharper disable ExcessiveIndentation
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

// https://stackoverflow.com/questions/2251260/how-to-develop-treeview-with-checkboxes-in-wpf
// https://stackoverflow.com/questions/14876032/group-dates-by-year-month-and-date-in-wpf-treeview
// https://www.codeproject.com/Articles/28306/Working-with-Checkboxes-in-the-WPF-TreeView

namespace FilterDataGrid
{
    public sealed class FilterCommon : NotifyProperty
    {
        #region Public Constructors

        public FilterCommon()
        {
            PreviouslyFilteredItems = new HashSet<object>(EqualityComparer<object>.Default);
        }

        #endregion Public Constructors

        #region Public Properties

        public string FieldName { get; set; }
        public Type FieldType { get; set; }
        public bool IsFiltered { get; set; }
        public HashSet<object> PreviouslyFilteredItems { get; set; }

        // Treeview
        public List<FilterItem> Tree { get; set; }

        public Loc Translate { get; set; }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        ///     Recursive call for check/uncheck all items in tree
        /// </summary>
        /// <param name="state"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        private void SetIsChecked(FilterItem item, bool? state, bool updateChildren, bool updateParent)
        {
            try
            {
                if (state == item.IsDateChecked) return;
                item.SetDateState = state;

                // select all / unselect all
                if (item.Level == 0)
                    Tree.Where(t => t.Level != 0).ToList().ForEach(c => { SetIsChecked(c, state, true, true); });

                if (updateChildren && item.IsDateChecked.HasValue)
                    item.Children?.ForEach(c => { SetIsChecked(c, state, true, false); });

                if (updateParent && item.Parent != null)
                    VerifyCheckedState(item.Parent);

                item.OnPropertyChanged("IsDateChecked");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetDateState : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Update the item tree when the state of the IsDateChecked property changes
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void UpdateTree(object o, bool? e)
        {
            if (o == null) return;
            var item = (FilterItem) o;
            SetIsChecked(item, e, true, true);
        }

        /// <summary>
        ///     check or uncheck parents or children
        /// </summary>
        private void VerifyCheckedState(FilterItem item)
        {
            bool? state = null;

            for (var i = 0; i < item.Children?.Count; ++i)
            {
                var current = item.Children[i].IsDateChecked;

                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(item, state, false, true);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        ///     Add the filter to the predicate dictionary
        /// </summary>
        public void AddFilter(Dictionary<string, Predicate<object>> criteria)
        {
            if (IsFiltered) return;

            bool Predicate(object o)
            {
                var value = o.GetType().GetProperty(FieldName)?.GetValue(o, null);
                return !PreviouslyFilteredItems.Contains(value);
            }

            criteria.Add(FieldName, Predicate);

            IsFiltered = true;
        }

        /// <summary>
        ///     Check if any tree item is checked (can apply filter)
        /// </summary>
        /// <returns></returns>
        public bool AnyDateIsChecked()
        {
            // any IsDateChecked is != false
            return Tree?.Skip(1).Any(t => t.IsDateChecked != false) ?? false;
        }

        /// <summary>
        ///     Build the item tree
        /// </summary>
        /// <param name="dates"></param>
        /// <param name="currentFilter"></param>
        /// <param name="uncheckPrevious"></param>
        /// <returns></returns>
        public IEnumerable<FilterItem> BuildTree(IEnumerable<object> dates, string lastFilter = null)
        {
            if (dates == null) return null;

            try
            {
                var dateTimes = dates.ToList();
                var uncheckPrevious = FieldName == lastFilter;

                Tree = new List<FilterItem>
                {
                    new FilterItem
                    {
                        Label = Translate.All, CurrentFilter = this, Content = 0, Level = 0, SetDateState = true
                    }
                };

                // event subscription
                Tree.First().OnIsCheckedDate += UpdateTree;

                // iterate over all items that are not null
                // INFO:
                // SetDateState  : does not raise OnIsCheckedDate event
                // IsDateChecked : raise OnIsCheckedDate event
                // (see the FilterItem class for more informations)

                foreach (var y in from date in dateTimes.Where(d => d != null)
                        .Select(d => (DateTime) d).OrderBy(o => o.Year)
                    group date by date.Year
                    into year
                    select new FilterItem
                    {
                        // YEAR
                        Level = 1,
                        CurrentFilter = this,
                        Content = year.Key,
                        Label = year.First().ToString("yyyy"),
                        SetDateState = true,

                        Children = (from date in year
                            group date by date.Month
                            into month
                            select new FilterItem
                            {
                                // MOUNTH
                                Level = 2,
                                CurrentFilter = this,
                                Content = month.Key,
                                Label = month.First().ToString("MMMM", Translate.Culture),
                                SetDateState = true,

                                Children = (from day in month
                                    select new FilterItem
                                    {
                                        // DAY
                                        Level = 3,
                                        CurrentFilter = this,
                                        Content = day.Day,
                                        Label = day.ToString("dd", Translate.Culture),
                                        SetDateState = true,
                                        Children = new List<FilterItem>()
                                    }).ToList()
                            }).ToList()
                    })
                {
                    y.OnIsCheckedDate += UpdateTree;

                    y.Children.ForEach(m =>
                    {
                        m.Parent = y;
                        m.OnIsCheckedDate += UpdateTree;

                        m.Children.ForEach(d =>
                        {
                            d.Parent = m;
                            d.OnIsCheckedDate += UpdateTree;

                            if (PreviouslyFilteredItems != null && uncheckPrevious)
                                d.IsDateChecked = PreviouslyFilteredItems
                                    .Any(u => u != null && u.Equals(new DateTime((int) y.Content, (int) m.Content,
                                        (int) d.Content))) == false;
                        });
                    });
                    Tree.Add(y);
                }

                // last empty item
                if (dateTimes.Any(d => d == null))
                    Tree.Add(
                        new FilterItem
                        {
                            Label = Translate.Empty, // translation
                            CurrentFilter = this,
                            Content = -1,
                            Level = -1,
                            SetDateState = !PreviouslyFilteredItems?.Any(u => u == null) == true,
                            Children = new List<FilterItem>()
                        }
                    );

                // event subscription if an empty element exists
                if (Tree.LastOrDefault(c => c.Level == -1) != null)
                    Tree.Last(c => c.Level == -1).OnIsCheckedDate += UpdateTree;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BuildTree : {ex.Message}");
                throw;
            }

            return Tree;
        }

        /// <summary>
        ///     Get all the items from the tree (checked or unchecked)
        /// </summary>
        /// <returns></returns>
        public List<FilterItem> GetAllItemsTree()
        {
            var filterCommon = new List<FilterItem>();

            try
            {
                foreach (var y in Tree.Skip(1))
                    if (y.Level > 0)
                        filterCommon.AddRange(
                            from m in y.Children
                            from d in m.Children
                            select new FilterItem
                            {
                                Content = new DateTime((int) y.Content, (int) m.Content, (int) d.Content),
                                IsChecked = d.IsDateChecked ?? false
                            });
                    else
                        filterCommon.Add(new FilterItem
                        {
                            Content = null,
                            IsChecked = y.IsDateChecked ?? false
                        });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetAllItemsTree : {ex.Message}");
                throw;
            }

            return filterCommon;
        }

        #endregion Public Methods
    }
}
