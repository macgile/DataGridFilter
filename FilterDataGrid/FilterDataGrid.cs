#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterDataGrid.cs
// Created    : 26/01/2021

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

// ReSharper disable ArrangeAccessorOwnerBody

// ReSharper disable InvertIf
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable CheckNamespace

// https://stackoverflow.com/questions/3685566/wpf-using-resizegrip-to-resize-controls
// https://www.c-sharpcorner.com/UploadFile/mahesh/binding-static-properties-in-wpf-4-5/
// https://www.csharp-examples.net/string-format-datetime/

// Tools.DiffFiles FilterDataGrid.cs FilterDataGrid\Properties\FilterDataGrid.cs

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

            // load resources
            Resources = new FilterDataGridDictionary();

            originalPopUpHeight = (double)FindResource("PopupHeight");

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
        ///     date format displayed
        /// </summary>
        public static readonly DependencyProperty DateFormatStringProperty =
            DependencyProperty.Register("DateFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata("d"));

        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local),
                typeof(FilterDataGrid),
                new PropertyMetadata(Local.English));

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

        #endregion Public DependencyProperty

        #region Public Event

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Sorted;

        #endregion Public Event

        #region Private Fields

        private const bool DebugMode = false;
        private static readonly Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;
        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();
        private Button button;
        private Type collectionType;
        private Cursor cursor;
        private TimeSpan elased;
        private string fieldName;
        private Geometry iconFilter;
        private Geometry iconFilterSet;
        private string lastFilter;
        private ListBox listBox;
        private double minHeight;
        private double minWidth;
        private double originalPopUpHeight;
        private Path pathFilterIcon;
        private bool pending;
        private Popup popup;
        private List<object> rawValuesDataGridItems;
        private bool search;
        private string searchText;
        private TextBox searchTextBox;
        private Grid sizableContentGrid;
        private double sizableContentHeight;
        private double sizableContentWidth;
        private List<object> sourceObjectList;
        private Thumb thumb;
        private TreeView treeview;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///     Date format displayed
        /// </summary>
        public string DateFormatString
        {
            get { return (string)GetValue(DateFormatStringProperty); }
            set { SetValue(DateFormatStringProperty, value); }
        }

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
        ///     Language
        /// </summary>
        public Local FilterLanguage
        {
            get { return (Local)GetValue(FilterLanguageProperty); }
            set { SetValue(FilterLanguageProperty, value); }
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
        ///     Instance of Loc
        /// </summary>
        public Loc Translate { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private ICollectionView CollectionViewSource { get; set; }
        private FilterCommon CurrentFilter { get; set; }
        private List<FilterCommon> GlobalFilterList { get; set; } = new List<FilterCommon>();
        private ICollectionView ItemCollectionView { get; set; }

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

                // FilterLanguage : default : 0 (english)
                Translate = new Loc { Language = (int)FilterLanguage };

                // sorting event
                Sorted += OnSorted;

                if (AutoGenerateColumns) return;

                // get the columns that can be filtered
                var columns = Columns
                    .Where(c => c is DataGridTextColumn dtx && dtx.IsColumnFiltered
                                || c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered)
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

                        // get type
                        var fieldType = Nullable.GetUnderlyingType(column.Binding.GetType()) ??
                                        column.Binding.GetType();

                        // apply DateFormatString when StringFormat for column is not provided
                        if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString) &&
                            string.IsNullOrEmpty(column.Binding.StringFormat))
                            column.Binding.StringFormat = DateFormatString;

                        // culture
                        ((Binding)column.Binding).ConverterCulture = Translate.Culture;
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
                    Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture /* StringFormat */},
                    FieldName = e.PropertyName,
                    Header = e.Column.Header.ToString(),
                    HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate"),
                    IsColumnFiltered = true
                };

                // get type
                var fieldType = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                // apply the format string provided
                if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                    column.Binding.StringFormat = DateFormatString;

                // culture
                ((Binding)column.Binding).ConverterCulture = Translate.Culture;

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
                if (GlobalFilterList.Count > 0)
                    criteria.Clear(); // clear criteria

                // remove filter and reset column
                foreach (var filter in GlobalFilterList)
                {
                    var coll = Columns
                        .FirstOrDefault(c => c is DataGridTextColumn dtx && dtx.FieldName == filter.FieldName
                                             || c is DataGridTemplateColumn dtp && dtp.FieldName == filter.FieldName);

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
                // thank's Stefan Heimel for this contribution
                if (CollectionViewSource.CanFilter)
                {
                    CollectionViewSource.Filter = Filter;
                }

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
        ///     Reset cursor
        /// </summary>
        private static async void ResetCursor()
        {
            // reset cursor
            await UiDispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = null; }),
                DispatcherPriority.ContextIdle);
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
        ///     Can show filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanShowFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CollectionViewSource?.CanFilter == true && (!popup?.IsOpen ?? true) && !pending;
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
                ItemCollectionView = null;
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

                sizableContentGrid.Height = originalPopUpHeight;
                sizableContentGrid.MinHeight = originalPopUpHeight;

                minHeight = sizableContentGrid.MinHeight;
                minWidth = sizableContentGrid.MinWidth;

                // Debug.WriteLine($"\nMinHeight: {minHeight, -8}Height :{sizableContentGrid.Height}\n");

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
                                    FieldType = fieldType,
                                    Translate = Translate
                                };

                // list of all item values, filtered and unfiltered (previous filtered items)
                sourceObjectList = new List<object>();

                // add the first element (select all)
                var filterItemList = new List<FilterItem>
                    {new FilterItem {Id = 0, Label = Translate.All, IsChecked = true}};

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

                    // rawValuesDataGridItems.RemoveAll(v => v == null || string.IsNullOrEmpty((string)v));

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    // displays new (checked) and already filtered (unchecked) items
                    // PreviouslyFilteredItems is a HashSet of objects
                    if (lastFilter == CurrentFilter.FieldName)
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems);

                    // sorting is a slow operation, using ParallelQuery
                    sourceObjectList = sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    var emptyItem = false;

                    // if it exists, place the empty element at the bottom of the list
                    if (sourceObjectList.Any(l => string.IsNullOrEmpty(l?.ToString())))
                    {
                        // the filter compare with Items values
                        // EmptyItem == null : items.value = ""  => the datagrid item is not filtered
                        // it is possible to have null values and "" in the same list
                        // if remove all these values and add a null value for EmptyItem, the values "" will not be found
                        emptyItem = true;
                        sourceObjectList.RemoveAll(v => v == null || string.IsNullOrEmpty(v.ToString()));
                    }

                    // add all items to the filterItemList
                    for (var i = 0; i < sourceObjectList.Count; i++)
                    {
                        var item = sourceObjectList[i];
                        var filterItem = new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = item, // raw value
                            Label = item?.ToString(), // Content displayed
                            Level = 1, // padding

                            // check or uncheck if the content of item exists in the previously filtered elements
                            IsChecked = !CurrentFilter?.PreviouslyFilteredItems.Contains(item) ?? false
                        };

                        filterItemList.Add(filterItem);
                    }

                    // add a empty item at the bottom of the list
                    if (emptyItem)
                    {
                        sourceObjectList.Insert(sourceObjectList.Count, null);

                        filterItemList.Add(new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = null,
                            Label = Translate.Empty,
                            IsChecked = !CurrentFilter?.PreviouslyFilteredItems.Contains(null) ?? false
                        });
                    }
                }); // and task

                // the current listbox or treeview
                if (fieldType == typeof(DateTime))
                {
                    treeview = VisualTreeHelpers.FindChild<TreeView>(popup.Child, "PopupTreeview");

                    if (treeview != null)
                    {
                        // fill the treeview with the tree structure built by CurrentFilter.BuildTree
                        // and if it is the last filter, uncheck the elements already filtered
                        treeview.ItemsSource =
                            CurrentFilter?.BuildTree(sourceObjectList, lastFilter /*== CurrentFilter.FieldName*/);
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

                // Set ICollectionView for filter in popup
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(filterItemList);

                // set filter in popup
                if (ItemCollectionView.CanFilter)
                {
                    ItemCollectionView.Filter = SearchFilter;
                }

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

            // list of content of items not to be filtered
            List<object> checkedItems;

            try
            {
                // list of content of items to filter
                var uncheckedItems = new List<object>();

                // to test if unchecked items are checked again
                var contain = false;

                // items already filtered
                var previousFilteredItems = new List<object>(CurrentFilter.PreviouslyFilteredItems);

                await Task.Run(() =>
                {
                    // filter date
                    if (CurrentFilter.FieldType == typeof(DateTime))
                    {
                        // get the list of dates from the treeview (any state : checked/not checked)
                        var dateList = CurrentFilter.GetAllItemsTree();

                        // items to be not filtered (checked)
                        checkedItems = dateList.Where(f => f.IsChecked).Select(f => f.Content).ToList();

                        // unchecked :
                        // the search result is checked => add unchecked elements to the unchecked list
                        // otherwise, add the unchecked items of the date list
                        uncheckedItems = search
                            ? rawValuesDataGridItems?.Except(checkedItems).ToList()
                            : dateList.Where(f => !f.IsChecked).Select(f => f.Content).ToList();
                    }
                    else
                    {
                        // all items listbox/treeview from popup
                        var viewItems = ItemCollectionView?.Cast<FilterItem>().Skip(1).ToList() ??
                                        new List<FilterItem>();

                        // items to be not filtered (checked)
                        checkedItems = viewItems.Where(f => f.IsChecked).Select(f => f.Content).ToList();

                        // unchecked :
                        // the search result is checked => add unchecked elements to the unchecked list
                        // otherwise add unchecked from the lisbox view
                        uncheckedItems = search
                            ? rawValuesDataGridItems.Except(checkedItems).ToList() // result search (only items checked)
                            : viewItems.Where(f => !f.IsChecked)
                                .Select(f => f.Content).ToList(); // items not checked

                        // two values, null and string.empty for list of string
                        if (uncheckedItems.Any(v => v == null))
                            uncheckedItems.Add(string.Empty);
                    }

                    if (checkedItems.Any() && uncheckedItems != null)
                    {
                        // check if unchecked (filtered) items have been checked
                        // common items (intersect) to the two lists = old items unchecked
                        contain = checkedItems.Intersect(previousFilteredItems).Any();

                        if (contain)
                        {
                            // remove string.Empty
                            if (checkedItems.Any(i => i == null))
                                previousFilteredItems.RemoveAll(item => item?.ToString() == string.Empty);

                            // remove filtered items that should no longer be filtered
                            previousFilteredItems = previousFilteredItems.Except(checkedItems).ToList();
                        }

                        // add the previous filtered items to the list of new items to filter
                        uncheckedItems.AddRange(previousFilteredItems);
                    }
                });

                if (uncheckedItems.Any() || contain)
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
        ///     PopUp placement and offset
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

                // get the host window of the datagrid
                // thank's Stefan Heimel for this contribution
                var hostingWindow = Window.GetWindow(this);

                if (hostingWindow != null)
                {
                    var mainHeight = hostingWindow.ActualHeight - 39d;
                    var mainWidth = hostingWindow.ActualWidth;
                    var col = ((DataGridColumnHeader)header).Column;

                    // get X,Y position
                    var headerMainPoint = header.TransformToVisual(hostingWindow).Transform(new Point(0, 0));
                    var popupHeaderPoint = popup.TransformToVisual(header).Transform(new Point(0, 0));
                    var headHeigth = header.ActualHeight;

                    var popupHeigth = originalPopUpHeight;
                    var popupWidth = grid.Width > 0d ? grid.Width : grid.ActualWidth;

                    // delta for max size popup
                    var deltaX = mainWidth - (headerMainPoint.X + popupWidth);
                    var deltaY = mainHeight - (headerMainPoint.Y + popupHeigth + headHeigth);

                    // first column
                    grid.MaxWidth = popupWidth + deltaX - 17d;

                    // the other columns
                    if (col.DisplayIndex > 0)
                    {
                        popup.HorizontalOffset = col.ActualWidth - popupWidth - popupHeaderPoint.X + 3d;
                        grid.MaxWidth += Math.Abs(popup.HorizontalOffset) - 1d;
                    }

                    // delta > 0 : main  > popup
                    // delta < 0 : popup > main
                    if (deltaY >= 0)
                    {
                        grid.MaxHeight = mainHeight - (headerMainPoint.Y + headHeigth) - 1d;
                    }
                    else
                    {
                        grid.MaxHeight = popupHeigth + deltaY;
                        grid.Height = grid.MaxHeight;
                        grid.MinHeight = grid.MaxHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PopupPlacement error : {ex.Message}");
                throw;
            }
        }

        #endregion Private Methods
    }

    /// <summary>
    ///     ResourceDictionary
    /// </summary>
    public partial class FilterDataGridDictionary
    {
        #region Public Constructors

        public FilterDataGridDictionary()
        {
            InitializeComponent();
        }

        #endregion Public Constructors
    }
}