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
                if (state == item.IsChecked) return;
                item.SetState = state;

                // select all / unselect all
                if (item.Level == 0)
                    Tree.Where(t => t.Level != 0).ToList().ForEach(c => { SetIsChecked(c, state, true, true); });

                // update children
                if (updateChildren && item.IsChecked.HasValue)
                    item.Children?.ForEach(c => { SetIsChecked(c, state, true, false); });

                // update parent
                if (updateParent && item.Parent != null)
                    VerifyCheckedState(item.Parent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.SetState : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Update the tree when the state of the IsChecked property changes
        /// </summary>
        /// <param name="o">item</param>
        /// <param name="e">state</param>
        public void UpdateTree(object o, bool? e)
        {
            if (o == null) return;
            var item = (FilterItem)o;
            SetIsChecked(item, e, true, true);
        }

        /// <summary>
        ///     Check or uncheck parents or children
        /// </summary>
        private void VerifyCheckedState(FilterItem item)
        {
            bool? state = null;

            for (var i = 0; i < item.Children?.Count; ++i)
            {
                var current = item.Children[i].IsChecked;

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

            // predicate of filter
            bool Predicate(object o)
            {
                var value = o.GetType().GetProperty(FieldName)?.GetValue(o, null);
                return !PreviouslyFilteredItems.Contains(value);
            }

            // add to list of predicates
            criteria.Add(FieldName, Predicate);

            IsFiltered = true;
        }

        /// <summary>
        ///    Any Date IsChecked, check if any tree item is checked (can apply filter)
        /// </summary>
        /// <returns></returns>
        public bool AnyDateIsChecked()
        {
            // any IsChecked is true or null
            // IsDate Checked has three states, isChecked: null and true
            return Tree != null && Tree.Skip(1).Any(t => t.IsChecked != false);
        }

        /// <summary>
        /// Any state of Date Changed, check if at least one date is checked and another is changed
        /// </summary>
        /// <returns></returns>
        public bool AnyDateChanged()
        {
            // any (year, month, day) status changed
            return Tree != null &&
                   Tree.Skip(1)
                       .Any(year => year.Changed || year.Children
                           .Any(month => month.Changed || month.Children
                               .Any(day => day.Changed))) && AnyDateIsChecked();
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

            try
            {
                var uncheckPrevious = FieldName == lastFilter;
                var type = typeof(DateTime);

                Tree = new List<FilterItem>
                {
                    new FilterItem(this)
                    {
                        Label = Translate.All, CurrentFilter = this, Content = 0, Level = 0, SetState = true, FieldType = type
                    }
                };

                if (dates == null) return Tree;
                // iterate over all items that are not null
                // INFO:
                // SetState : does not raise OnDateStatusChanged event
                // IsChecked    : raise OnDateStatusChanged event
                // (see the FilterItem class for more informations)
               
                var dateTimes = dates.ToList();

                foreach (var y in from date in dateTimes.Where(d => d != null)
                        .Select(d => (DateTime)d).OrderBy(o => o.Year)
                                  group date by date.Year into year
                                  select new FilterItem(this)
                                  {
                                      // YEAR
                                      Level = 1,
                                      CurrentFilter = this,
                                      Content = year.Key,
                                      Label = year.First().ToString("yyyy", Translate.Culture),
                                      SetState = true, // default state
                                      FieldType = type,

                                      Children = (from date in year
                                                  group date by date.Month into month
                                                  select new FilterItem(this)
                                                  {
                                                      // MOUNTH
                                                      Level = 2,
                                                      CurrentFilter = this,
                                                      Content = month.Key,
                                                      Label = month.First().ToString("MMMM", Translate.Culture),
                                                      SetState = true, // default state
                                                      FieldType = type,

                                                      Children = (from day in month
                                                                  select new FilterItem(this)
                                                                  {
                                                                      // DAY
                                                                      Level = 3,
                                                                      CurrentFilter = this,
                                                                      Content = day.Day,
                                                                      Label = day.ToString("dd", Translate.Culture),
                                                                      SetState = true, // default state
                                                                      FieldType = type,
                                                                      Children = new List<FilterItem>()
                                                                  }).ToList()
                                                  }).ToList()
                                  })
                {
                    // set parent and IsChecked property if uncheckPrevious items
                    y.Children.ForEach(m =>
                    {
                        m.Parent = y;

                        m.Children.ForEach(d =>
                        {
                            d.Parent = m;

                            // set the state of the ischecked property based on the items already filtered (unchecked)
                            if (PreviouslyFilteredItems != null && uncheckPrevious)
                                d.IsChecked = PreviouslyFilteredItems
                                    .Any(u => u != null && u.Equals(new DateTime((int)y.Content, (int)m.Content, (int)d.Content))) == false;

                            // reset initialization with new state
                            d.InitialState = d.IsChecked;
                        });

                        // reset initialization with new state
                        m.InitialState = m.IsChecked;
                    });

                    // reset initialization with new state
                    y.InitialState = y.IsChecked;

                    Tree.Add(y);
                }

                // last empty item if exist in collection
                if (dateTimes.Any(d => d == null))
                    Tree.Add(
                        new FilterItem(this)
                        {
                            Label = Translate.Empty, // translation
                            CurrentFilter = this,
                            Content = null,
                            Level = -1,
                            FieldType = type,
                            SetState = PreviouslyFilteredItems?.Any(u => u == null) == false,
                            Children = new List<FilterItem>()
                        }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }

            return Tree;
        }

        /// <summary>
        ///     Get all the items from the tree (checked or unchecked)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FilterItem> GetAllItemsTree()
        {
            var filterCommon = new List<FilterItem>();

            try
            {
                // skip first item (select all)
                foreach (var y in Tree.Skip(1))
                    if (y.Level > 0) // year :1, mounth : 2, day : 3
                        filterCommon.AddRange(
                            from m in y.Children
                            from d in m.Children
                            select new FilterItem
                            {
                                Content = new DateTime((int)y.Content, (int)m.Content, (int)d.Content),
                                IsChecked = d.IsChecked ?? false,
                            });
                    else // null date (Level -1)
                        filterCommon.Add(new FilterItem
                        {
                            Content = null,
                            IsChecked = y.IsChecked ?? false,
                        });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.GetAllItemsTree : {ex.Message}");
                throw;
            }

            return filterCommon;
        }

        #endregion Public Methods
    }
}