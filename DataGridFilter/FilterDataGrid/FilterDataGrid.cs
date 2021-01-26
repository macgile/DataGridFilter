#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterDataGrid.cs
// Created    : 08/12/2020
//

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

// ReSharper disable InvertIf
// ReSharper disable RedundantAssignment
// ReSharper disable ClassTooBig
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeThisQualifier
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable CheckNamespace

// https://stackoverflow.com/questions/3685566/wpf-using-resizegrip-to-resize-controls
// https://www.c-sharpcorner.com/UploadFile/mahesh/binding-static-properties-in-wpf-4-5/

namespace FilterDataGrid
{
    /// <summary>
    ///     Implementation of Datagrid
    /// </summary>
    public sealed class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region Public Constructors

        public FilterDataGrid()
        {
            Debug.WriteLineIf(DebugMode, "Constructor");

            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand, CanShowFilter));
            CommandBindings.Add(new CommandBinding(ApplyFilter, ApplyFilterCommand, CanApplyFilter)); // Ok
            CommandBindings.Add(new CommandBinding(CancelFilter, CancelFilterCommand));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterCommand, CanRemoveFilter));
            CommandBindings.Add(new CommandBinding(IsChecked, CheckedAllCommand));
            CommandBindings.Add(new CommandBinding(ClearSearchBox, ClearSearchBoxClick));
        }

        #endregion Public Constructors

        #region Command

        public static readonly ICommand ApplyFilter = new RoutedCommand();

        public static readonly ICommand CancelFilter = new RoutedCommand();

        public static readonly ICommand ClearSearchBox = new RoutedCommand();

        public static readonly ICommand IsChecked = new RoutedCommand();

        public static readonly ICommand RemoveFilter = new RoutedCommand();

        public static readonly ICommand ShowFilter = new RoutedCommand();

        #endregion Command

        #region Public DependencyProperty

        /// <summary>
        ///     Show elapsed time (for debug)
        /// </summary>
        public static readonly DependencyProperty ShowElapsedTimeProperty =
            DependencyProperty.Register("ShowElapsedTime",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Show statusbar
        /// </summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register("ShowStatusBar",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     date format displayed
        /// </summary>
        public static readonly DependencyProperty DateFormatStringProperty =
            DependencyProperty.Register("DateFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata("d"));

        #endregion Public DependencyProperty

        #region Public Event

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Sorted;

        #endregion Public Event

        #region Private Fields

        private const bool DebugMode = false;
        private bool pending;
        private bool search;

        private TimeSpan elased;

        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;

        private Geometry iconFilter;
        private Geometry iconFilterSet;

        private List<object> sourceObjectList;
        private List<object> rawValuesDataGridItems;

        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();

        private string fieldName;
        private string lastFilter;
        private string searchText;

        private Button button;
        private Cursor cursor;
        private Grid sizableContentGrid;
        private ListBox listBox;
        private Path pathFilterIcon;
        private Popup popup;
        private TextBox searchTextBox;
        private Thumb thumb;
        private TreeView treeview;
        private Type collectionType;
        private static readonly Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///     Elapsed time
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get => elased;
            set
            {
                elased = value;

                //Debug.WriteLine("OnPropertyChanged");
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Display items count
        /// </summary>
        public int ItemsSourceCount { get; set; }

        public bool ShowElapsedTime
        {
            get { return (bool)GetValue(ShowElapsedTimeProperty); }
            set { SetValue(ShowElapsedTimeProperty, value); }
        }

        /// <summary>
        ///     Show status bar
        /// </summary>
        public bool ShowStatusBar
        {
            get { return (bool)GetValue(ShowStatusBarProperty); }
            set { SetValue(ShowStatusBarProperty, value); }
        }

        /// <summary>
        ///     Date format displayed
        /// </summary>
        public string DateFormatString
        {
            get { return (string)GetValue(DateFormatStringProperty); }
            set { SetValue(DateFormatStringProperty, value); }
        }

        #endregion Public Properties

        #region Private Properties

        private FilterCommon CurrentFilter { get; set; }
        private ICollectionView ItemCollectionView { get; set; }
        private ICollectionView CollectionViewSource { get; set; }
        private List<FilterCommon> GlobalFilterList { get; set; } = new List<FilterCommon>();

        #endregion Private Properties

        #region Protected Methods

        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnInitialized");

            base.OnInitialized(e);

            try
            {
                iconFilterSet = (Geometry)FindResource("FilterSet");
                iconFilter = (Geometry)FindResource("Filter");

                // sorting event
                Sorted += OnSorted;

                if (AutoGenerateColumns) return;

                // get the columns that can be filtered
                var columns = Columns
                    .Where(c => c.GetType() == typeof(DataGridTextColumn) && ((DataGridTextColumn)c).IsColumnFiltered
                                || c.GetType() == typeof(DataGridTemplateColumn) &&
                                ((DataGridTemplateColumn)c).IsColumnFiltered)
                    .Select(c => c)
                    .ToList();

                // set header template
                foreach (var col in columns)
                {
                    var columnType = col.GetType();

                    if (columnType == typeof(DataGridTextColumn))
                    {
                        var column = (DataGridTextColumn)col;
                        column.HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate");
                        column.FieldName = ((Binding)column.Binding).Path.Path;
                    }
                    else if (columnType == typeof(DataGridTemplateColumn))
                    {
                        var column = (DataGridTemplateColumn)col;
                        column.HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnInitialized : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Auto generated column, set templateHeader
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnAutoGeneratingColumn");
            base.OnAutoGeneratingColumn(e);

            try
            {
                if (e.Column.GetType() != typeof(System.Windows.Controls.DataGridTextColumn)) return;

                var column = new DataGridTextColumn
                {
                    Binding = new Binding(e.PropertyName),
                    FieldName = e.PropertyName,
                    Header = e.Column.Header.ToString(),
                    HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate"),
                    IsColumnFiltered = true
                };

                e.Column = column;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnAutoGeneratingColumn : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     The source of the Datagrid items has been changed (refresh or on loading)
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Debug.WriteLineIf(DebugMode, "OnItemsSourceChanged");

            // order call :
            // Constructor
            // OnInitialized
            // OnItemsSourceChanged

            base.OnItemsSourceChanged(oldValue, newValue);

            try
            {
                // remove the filter set and reset the column
                if (GlobalFilterList.Count > 0)
                    criteria.Clear(); // clear filter

                foreach (var filter in GlobalFilterList)
                {
                    var coll = Columns
                        .FirstOrDefault(c => c.GetType() == typeof(DataGridTextColumn)
                                             && ((DataGridTextColumn)c).FieldName == filter.FieldName
                                             || c.GetType() == typeof(DataGridTemplateColumn)
                                             && ((DataGridTemplateColumn)c).FieldName == filter.FieldName);

                    if (coll == null) continue;

                    var header = VisualTreeHelpers.GetHeader(coll, this);
                    var buttonFilter = header?.FindVisualChild<Button>();

                    if (buttonFilter == null) continue;

                    buttonFilter.Opacity = 0.5;
                    pathFilterIcon = VisualTreeHelpers.FindChild<Path>(buttonFilter, "PathFilterIcon");
                    if (pathFilterIcon != null)
                        pathFilterIcon.Data = iconFilter;
                }

                CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);
                // set Filter
                CollectionViewSource.Filter = Filter;

                GlobalFilterList = new List<FilterCommon>();
                ItemsSourceCount = Items.Count;
                OnPropertyChanged("ItemsSourceCount");
                ElapsedTime = new TimeSpan(0, 0, 0);

                // if there is no item in ItemsSource, the Cast fails and an error occurs
                if (ItemsSourceCount > 0)
                    collectionType = ItemsSource?.Cast<object>().First().GetType();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnItemsSourceChanged : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Set the cursor to "Cursors.Wait" during a long sorting operation
        ///     https://stackoverflow.com/questions/8416961/how-can-i-be-notified-if-a-datagrid-column-is-sorted-and-not-sorting
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            if (pending) return;
            Mouse.OverrideCursor = Cursors.Wait;
            base.OnSorting(eventArgs);
            Sorted?.Invoke(this, new EventArgs());
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        ///     Reset the cursor at the end of the sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSorted(object sender, EventArgs e)
        {
            ResetCursor();
        }

        /// <summary>
        ///     Can show filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanShowFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (!popup?.IsOpen ?? true) && !pending;
        }

        /// <summary>
        ///     Can Apply filter (popup Ok button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanApplyFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentFilter?.FieldType == typeof(DateTime)
                ? CurrentFilter.AnyDateIsChecked() // treeview
                : ItemCollectionView?.Cast<FilterItem>().Skip(1).Any(f => f.IsChecked) ?? false;
        }

        /// <summary>
        ///     Cancel button, close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (popup == null) return;
            popup.IsOpen = false; // raise EventArgs PopupClosed
        }

        /// <summary>
        ///     Can remove filter when current column (CurrentFilter) filtered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentFilter?.IsFiltered ?? false;
        }

        /// <summary>
        ///     Clear Search Box text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void ClearSearchBoxClick(object sender, RoutedEventArgs routedEventArgs)
        {
            search = false;
            searchTextBox.Text = string.Empty; // raises TextChangedEventArgs
        }

        /// <summary>
        ///     Check/uncheck all item when the action is (select all)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (FilterItem)e.Parameter;

            // only when the item[0] (select all) is checked or unchecked
            if (item?.Id != 0 || ItemCollectionView == null) return;

            foreach (var obj in ItemCollectionView?.Cast<FilterItem>().Skip(1)
                .Where(f => f.IsChecked != item.IsChecked))
                obj.IsChecked = item.IsChecked;
        }

        /// <summary>
        ///     On Resize Thumb Drag Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Cursor = cursor;
        }

        /// <summary>
        ///     Get delta on drag thumb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            // initialize the first Actual size Width/Height
            if (sizableContentHeight <= 0)
            {
                sizableContentHeight = sizableContentGrid.ActualHeight;
                sizableContentWidth = sizableContentGrid.ActualWidth;
            }

            var yAdjust = sizableContentGrid.Height + e.VerticalChange;
            var xAdjust = sizableContentGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or heigth
            xAdjust = sizableContentGrid.ActualWidth + xAdjust > minWidth ? xAdjust : minWidth;
            yAdjust = sizableContentGrid.ActualHeight + yAdjust > minHeight ? yAdjust : minHeight;

            xAdjust = xAdjust < minWidth ? minWidth : xAdjust;
            yAdjust = yAdjust < minHeight ? minHeight : yAdjust;

            // set size of grid
            sizableContentGrid.Width = xAdjust;
            sizableContentGrid.Height = yAdjust;
        }

        /// <summary>
        ///     On Resize Thumb DragStarted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            cursor = Cursor;
            Cursor = Cursors.SizeNWSE;
        }

        /// <summary>
        ///     Remove current filter
        /// </summary>
        private void RemoveCurrentFilter()
        {
            if (CurrentFilter == null) return;

            popup.IsOpen = false;
            button.Opacity = 0.5;
            pathFilterIcon.Data = iconFilter;

            var start = DateTime.Now;
            ElapsedTime = new TimeSpan(0, 0, 0);

            Mouse.OverrideCursor = Cursors.Wait;

            if (CurrentFilter.IsFiltered && criteria.Remove(CurrentFilter.FieldName))
                CollectionViewSource.Refresh();

            if (GlobalFilterList.Contains(CurrentFilter))
                GlobalFilterList.Remove(CurrentFilter);

            // set the last filter applied
            lastFilter = GlobalFilterList.LastOrDefault()?.FieldName;

            ElapsedTime = DateTime.Now - start;

            ResetCursor();
        }

        /// <summary>
        ///     remove current filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCurrentFilter();
        }

        /// <summary>
        ///     Filter current list in popup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            var item = (FilterItem)obj;
            if (string.IsNullOrEmpty(searchText) || item == null || item.Id == 0) return true;

            if (item.FieldType == typeof(DateTime))
                return ((DateTime?)item.Content)?.ToString("d")
                    .IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            return item.Content?.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Search TextBox Text Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            // fix TextChanged event fires twice
            // I did not find another solution
            if (textBox == null || textBox.Text == searchText || ItemCollectionView == null) return;

            searchText = textBox.Text;

            // Debug.WriteLine($"{searchText} == {textBox?.Text}");

            search = !string.IsNullOrEmpty(searchText);

            // apply filter
            ItemCollectionView.Refresh();

            if (CurrentFilter.FieldType != typeof(DateTime) || treeview == null) return;

            // rebuild treeview
            var items = ItemCollectionView?.Cast<FilterItem>()
                .Skip(1) // skip (select all)
                .Where(i => i.IsChecked)
                .Select(f => f.Content)
                .Distinct()
                .ToList();

            if (items.Count > 0 && !string.IsNullOrEmpty(searchText))
                treeview.ItemsSource = CurrentFilter.BuildTree(items);
            else if (string.IsNullOrEmpty(searchText))
                // fill the tree with the elements of the list of the original items
                treeview.ItemsSource = CurrentFilter.BuildTree(sourceObjectList);
        }

        /// <summary>
        ///     Open popup on Button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nShowFilterCommand");

            // reset previous  elapsed time
            ElapsedTime = new TimeSpan(0, 0, 0);
            var start = DateTime.Now;

            // clear search text (important)
            searchText = string.Empty;
            search = false;

            try
            {
                button = (Button)e.OriginalSource;

                if (Items.Count == 0 || button == null) return;

                // navigate up to the current header and get column type
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                var columnType = header.Column.GetType();

                // then down to the current popup
                popup = VisualTreeHelpers.FindChild<Popup>(header, "FilterPopup");

                if (popup == null) return;

                // popup handle event
                popup.Closed -= PopupClosed;
                popup.Closed += PopupClosed;

                // icon filter
                pathFilterIcon = VisualTreeHelpers.FindChild<Path>(button, "PathFilterIcon");

                // resizable grid
                sizableContentGrid = VisualTreeHelpers.FindChild<Grid>(popup.Child, "SizableContentGrid");

                // search textbox
                searchTextBox = VisualTreeHelpers.FindChild<TextBox>(popup.Child, "SearchBox");
                searchTextBox.Text = string.Empty;
                searchTextBox.TextChanged += SearchTextBoxOnTextChanged;

                // clear SearchBox button
                var clearSearchBoxBtn = VisualTreeHelpers.FindChild<Button>(popup.Child, "ClearSearchBoxBtn");
                clearSearchBoxBtn.Click += ClearSearchBoxClick;

                // thumb resize grip
                thumb = VisualTreeHelpers.FindChild<Thumb>(sizableContentGrid, "PopupThumb");

                // minimum size of Grid
                sizableContentHeight = 0;
                sizableContentWidth = 0;
                minHeight = sizableContentGrid.MinHeight;
                minWidth = sizableContentGrid.MinWidth;

                // thumb handle event
                thumb.DragCompleted += OnResizeThumbDragCompleted;
                thumb.DragDelta += OnResizeThumbDragDelta;
                thumb.DragStarted += OnResizeThumbDragStarted;

                // get field name from binding Path
                if (columnType == typeof(DataGridTextColumn))
                {
                    var column = (DataGridTextColumn)header.Column;
                    fieldName = column.FieldName;
                }

                if (columnType == typeof(DataGridTemplateColumn))
                {
                    var column = (DataGridTemplateColumn)header.Column;
                    fieldName = column.FieldName;
                }

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                // get type of field
                Type fieldType = null;
                var fieldProperty = collectionType.GetProperty(fieldName);

                if (fieldProperty != null)
                    // get type or get underlying type if nullable
                    fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;

                // If no filter, add filter to GlobalFilterList list
                CurrentFilter = GlobalFilterList.FirstOrDefault(f => f.FieldName == fieldName) ??
                                new FilterCommon
                                {
                                    FieldName = fieldName,
                                    FieldType = fieldType
                                };

                // list of all item values, filtered and unfiltered (previous filtered items)
                sourceObjectList = new List<object>();

                // add the first element (select all)
                var filterItemList = new List<FilterItem>
                    {new FilterItem {Id = 0, Label = Loc.All, IsChecked = true}};

                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                // get the list of distinct values from the selected column
                // List of raw values of the current column
                await Task.Run(() =>
                {
                    sourceObjectList = Items.Cast<object>()
                        .Select(x => x.GetType().GetProperty(fieldName)?.GetValue(x, null))
                        .Distinct() // clear duplicate values first
                        .Select(item => item)
                        .ToList();

                    // only the raw values of the items of the datagrid view
                    rawValuesDataGridItems = new List<object>(sourceObjectList);

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    // displays new (checked) and already filtered (unchecked) items
                    // PreviouslyFilteredItems is a list of objects that can be a string or a datetime
                    if (lastFilter == CurrentFilter.FieldName && CurrentFilter?.PreviouslyFilteredItems.Any() == true)
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems);

                    // sorting is a slow operation, using ParallelQuery
                    sourceObjectList = sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    var containtsEmpty = false;

                    // if it exists, place the empty element at the bottom of the list
                    if (sourceObjectList.Any(l => string.IsNullOrEmpty(l?.ToString())))
                    {
                        containtsEmpty = true;
                        var emptyItems = sourceObjectList.Where(x => string.IsNullOrEmpty(x?.ToString())).ToList();
                        foreach (var em in emptyItems) sourceObjectList.Remove(em);
                    }

                    // add all items to the "observable" collection
                    // if (fieldType != typeof(DateTime))
                    for (var i = 0; i < sourceObjectList.Count; i++)
                    {
                        var item = sourceObjectList[i];
                        var filterItem = new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = item, // raw value
                            Label = item?.ToString(), // Content displayed

                            // check or uncheck if the content of item exists in the previously filtered elements
                            IsChecked = !CurrentFilter?.PreviouslyFilteredItems.Contains(item) ?? false,
                        };

                        filterItemList.Add(filterItem);
                    }

                    // add a empty item at the bottom of the list
                    if (containtsEmpty)
                    {
                        sourceObjectList.Insert(sourceObjectList.Count, null);

                        filterItemList.Add(new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = null,
                            Label = Loc.Empty,
                            IsChecked = !CurrentFilter?.PreviouslyFilteredItems.Contains(null) ?? false
                        });
                    }
                }); // and task

                // the current listbox or tree structure
                if (fieldType == typeof(DateTime))
                {
                    treeview = VisualTreeHelpers.FindChild<TreeView>(popup.Child, "PopupTreeview");

                    if (treeview != null)
                    {
                        // fill tree with list
                        // and if it is the last filter, uncheck the elements already filtered
                        treeview.ItemsSource = CurrentFilter?.BuildTree(sourceObjectList, lastFilter == CurrentFilter.FieldName);
                        treeview.Visibility = Visibility.Visible;
                    }

                    if (listBox != null)
                    {
                        // clear previous data
                        listBox.ItemsSource = null;
                        listBox.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    listBox = VisualTreeHelpers.FindChild<ListBox>(popup.Child, "PopupListBox");
                    if (listBox != null)
                    {
                        // set filterList as ItemsSource of ListBox
                        listBox.Visibility = Visibility.Visible;
                        listBox.ItemsSource = filterItemList;
                        listBox.UpdateLayout();

                        // scroll to top of view
                        var scrollViewer =
                            VisualTreeHelpers.GetDescendantByType(listBox, typeof(ScrollViewer)) as ScrollViewer;
                        scrollViewer?.ScrollToTop();
                    }

                    if (treeview != null)
                    {
                        // clear previous data
                        treeview.ItemsSource = null;
                        treeview.Visibility = Visibility.Collapsed;
                    }
                }

                // Set CollectionView
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(filterItemList);

                // set filter in popup
                ItemCollectionView.Filter = SearchFilter;

                // set the placement and offset of the PopUp in relation to the header and
                // the main window of the application (placement : bottom left or bottom right)
                PopupPlacement(sizableContentGrid, header);

                // open popup
                popup.IsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // show open popup elapsed time in UI
                ElapsedTime = DateTime.Now - start;

                // reset cursor
                ResetCursor();
            }
        }

        /// <summary>
        ///     Click OK Button when Popup is Open, apply filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nApplyFilterCommand");

            var start = DateTime.Now;
            pending = true;
            popup.IsOpen = false; // raise PopupClosed event

            // set cursor wait
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // unchecked items = list of the content of the items to filter
                var uncheckedItems = new List<object>();
                // checked items = list of the content of items not to be filtered
                var checkedItems = new List<object>();

                // to test if unchecked items are checked again
                var contain = false;
                // items already filtered
                var previousFilteredItems = new List<object>(CurrentFilter.PreviouslyFilteredItems);

                await Task.Run(() =>
                {
                    // all items listbox/treeview from popup
                    var viewItems = ItemCollectionView?.Cast<FilterItem>().Skip(1).ToList() ?? new List<FilterItem>();

                    // filter date
                    if (CurrentFilter.FieldType == typeof(DateTime))
                    {
                        // get the list of dates from the treeview (any state : checked/not checked)
                        var dateList = CurrentFilter.GetAllItemsTree();

                        // items to be not filtered (checked)
                        checkedItems = dateList.Where(f => f.IsChecked).Select(f => f.Content).ToList();

                        // unchecked :
                        // result search is checked => add to unchecked
                        // otherwise, add the unchecked items of the date list
                        uncheckedItems = search
                            ? rawValuesDataGridItems?.Except(checkedItems).ToList()
                            : dateList.Where(f => !f.IsChecked).Select(f => f.Content).ToList();
                    }
                    else
                    {
                        // items to be not filtered (checked)
                        checkedItems = viewItems.Where(f => f.IsChecked).Select(f => f.Content).ToList();

                        // unchecked :
                        // result search is checked => add to unchecked
                        // otherwise add unchecked from the lisbox view
                        uncheckedItems = search
                            ? rawValuesDataGridItems // only items displayed in datagrid view
                                .Except(viewItems.Where(v => v.IsChecked).Select(v => v.Content))
                                .ToList() // result search (only items checked)
                            : viewItems.Where(f => !f.IsChecked).Select(f => f.Content)
                                .ToList(); // items not checked
                    }

                    if (checkedItems != null && uncheckedItems != null)
                    {
                        // check if unchecked (filtered) items have been checked
                        // common items (intersect) to the two lists = old items unchecked
                        contain = checkedItems.Intersect(previousFilteredItems).Any();

                        if (contain)
                        {
                            // remove filtered items that should no longer be filtered
                            previousFilteredItems = previousFilteredItems.Except(checkedItems).ToList();

                            // add the previous filtered items to the list of new items to filter
                            uncheckedItems.AddRange(previousFilteredItems);
                        }
                    }
                });

                if ((uncheckedItems.Any() || contain) && CurrentFilter != null)
                {
                    // fill the PreviouslyFilteredItems HashSet with unchecked items
                    CurrentFilter.PreviouslyFilteredItems =
                        new HashSet<object>(uncheckedItems, EqualityComparer<object>.Default);

                    // add a filter if it is not already added previously
                    if (!CurrentFilter.IsFiltered)
                        CurrentFilter.AddFilter(criteria);

                    // add current filter to GlobalFilterList
                    if (GlobalFilterList.All(f => f.FieldName != CurrentFilter.FieldName))
                        GlobalFilterList.Add(CurrentFilter);

                    // set the current field name as the last filter name
                    lastFilter = CurrentFilter.FieldName;

                    // set button opacity
                    button.Opacity = 1;

                    // set icon filter
                    pathFilterIcon.Data = iconFilterSet;

                    // apply filter
                    CollectionViewSource.Refresh();

                    // remove the current filter if there is no items to filter
                    if (!CurrentFilter.PreviouslyFilteredItems.Any())
                        RemoveCurrentFilter();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                pending = false;
                ResetCursor();
                ElapsedTime = elased.Add(DateTime.Now - start);

                Debug.WriteLineIf(DebugMode, $"Elapsed time : m:{ElapsedTime.Minutes} s:{ElapsedTime.Seconds}\r\n");
            }
        }

        /// <summary>
        ///     Reset the size of popup to original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupClosed(object sender, EventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nPopupClosed");

            var pop = (Popup)sender;

            // clear lists if close popup without filtering
            if (!pending)
            {
                // clear resource
                sourceObjectList = null;
                rawValuesDataGridItems = null;
            }

            sizableContentGrid.Width = sizableContentWidth;
            sizableContentGrid.Height = sizableContentHeight;
            Cursor = cursor;

            // fix resize grip, unsubscribe the event
            if (pop != null)
                pop.Closed -= PopupClosed;

            thumb.DragCompleted -= OnResizeThumbDragCompleted;
            thumb.DragDelta -= OnResizeThumbDragDelta;
            thumb.DragStarted -= OnResizeThumbDragStarted;
        }

        /// <summary>
        ///     Fix PopUp placement and offset
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="header"></param>
        private void PopupPlacement(FrameworkElement grid, FrameworkElement header)
        {
            try
            {
                popup.PlacementTarget = header;
                popup.HorizontalOffset = -1d;
                popup.VerticalOffset = -1d;
                popup.Placement = PlacementMode.Bottom;

                // main window
                var mainWindow = Application.Current.MainWindow;

                if (mainWindow == null) return;

                var popupPoint = popup.TransformToVisual(mainWindow).Transform(new Point(0, 0));
                var popupWidth = grid.Width > 0
                    ? grid.Width
                    : grid.ActualWidth;

                var delta = popupPoint.X + popupWidth - (mainWindow.ActualWidth - 16d);
                var offset = Math.Abs(popupWidth - header.ActualWidth) * -1d;

                if (delta > 0d)
                    popup.HorizontalOffset = offset - 2d;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PopupPlacement error : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Aggregate list of predicate as filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool Filter(object o)
        {
            return criteria.Values
                .Aggregate(true, (prevValue, predicate) => prevValue && predicate(o));
        }

        /// <summary>
        ///     OnPropertyChange
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Reset cursor
        /// </summary>
        private static async void ResetCursor()
        {
            // reset cursor
            await UiDispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = null; }),
                DispatcherPriority.ContextIdle);
        }

        #endregion Private Methods
    }
}