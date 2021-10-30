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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InvertIf
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable CheckNamespace

// https://stackoverflow.com/questions/3685566/wpf-using-resizegrip-to-resize-controls
// https://www.c-sharpcorner.com/UploadFile/mahesh/binding-static-properties-in-wpf-4-5/
// https://www.csharp-examples.net/string-format-datetime/

namespace FilterDataGrid
{
    /// <summary>
    ///     Implementation of Datagrid
    /// </summary>
    public sealed class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region Public Constructors

        /// <summary>
        ///  FilterDataGrid constructor
        /// </summary>
        public FilterDataGrid()
        {
            Debug.WriteLineIf(DebugMode, "Constructor");

            // load resources
            Resources = new FilterDataGridDictionary();

            // initial popup size
            popUpSize = new Point
            {
                X = (double)FindResource("PopupWidth"),
                Y = (double)FindResource("PopupHeight")
            };

            // icons
            iconFilterSet = (Geometry)FindResource("FilterSet");
            iconFilter = (Geometry)FindResource("Filter");

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

        /// <summary>
        /// Language displayed
        /// </summary>
        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local?),
                typeof(FilterDataGrid),
                new PropertyMetadata(null));

        /// <summary>
        ///     Show elapsed time in status bar
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

        private bool pending;
        private bool search;
        private Button button;
        private const bool DebugMode = false;
        private Cursor cursor;
        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;
        private Grid sizableContentGrid;

        private List<object> sourceObjectList;

        private ListBox listBox;
        private Path pathFilterIcon;
        private Point popUpSize;
        private Popup popup;
        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();
        private readonly Geometry iconFilter;
        private readonly Geometry iconFilterSet;
        private static readonly Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;
        private string fieldName;
        private string lastFilter;
        private string searchText;
        private TextBox searchTextBox;
        private Thumb thumb;
        private TimeSpan elased;
        private TreeView treeview;
        private Type collectionType;

        private object currentColumn;

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
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Language
        /// </summary>
        public Local? FilterLanguage
        {
            get { return (Local?)GetValue(FilterLanguageProperty); }
            set { SetValue(FilterLanguageProperty, value); }
        }

        /// <summary>
        ///     Display items count
        /// </summary>
        public int ItemsSourceCount { get; set; }

        /// <summary>
        /// Show elapsed time in status bar
        /// </summary>
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
        private IEnumerable<FilterItem> PopupViewItems => ItemCollectionView?.Cast<FilterItem>().Skip(1) ?? new List<FilterItem>();

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
                // FilterLanguage : default : CurrentCulture or English
                Translate = new Loc(FilterLanguage);
                FilterLanguage = (Local)Translate.Language;

                // sorting event
                Sorted += OnSorted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnInitialized : {ex.Message}");
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

                e.Column = column;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnAutoGeneratingColumn : {ex.Message}");
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
                if (newValue == null) return;

                GlobalFilterList = new List<FilterCommon>();
                criteria.Clear(); // clear criteria

                CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);

                // set Filter
                // thank's Stefan Heimel for this contribution
                if (CollectionViewSource.CanFilter)
                {
                    CollectionViewSource.Filter = Filter;
                }

                ItemsSourceCount = Items.Count;
                OnPropertyChanged("ItemsSourceCount");

                ElapsedTime = new TimeSpan(0, 0, 0);

                // get collection type
                if (ItemsSourceCount > 0)
                {
                    // Apflkuacha contribution
                    if (ItemsSource is ICollectionView collectionView)
                    {
                        collectionType = collectionView.SourceCollection?.GetType().GenericTypeArguments?.FirstOrDefault();
                    }
                    else
                    {
                        collectionType = ItemsSource?.GetType().GenericTypeArguments?.FirstOrDefault();
                    }
                }

                // generating custom columns
                if (!AutoGenerateColumns && collectionType != null)
                    GeneratingCustomsColumn();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnItemsSourceChanged : {ex.Message}");
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
            if (pending || (popup?.IsOpen ?? false)) return;

            Mouse.OverrideCursor = Cursors.Wait;
            base.OnSorting(eventArgs);
            Sorted?.Invoke(this, new EventArgs());
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Generate custom columns that can be filtered
        /// </summary>
        private void GeneratingCustomsColumn()
        {
            Debug.WriteLineIf(DebugMode, "GeneratingCustomColumn");

            try
            {
                // get the columns that can be filtered
                var columns = Columns
                    .Where(c => c is DataGridTextColumn dtx && dtx.IsColumnFiltered || c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered)
                    .Select(c => c)
                    .ToList();

                // set header template
                foreach (var col in columns)
                {
                    var columnType = col.GetType();

                    // reset icon filter
                    // for some reason the button icon doesn't reset when the template is reloaded,
                    // I could not find other solution to this problem.
                    var header = VisualTreeHelpers.GetHeader(col, this);
                    var buttonFilter = header?.FindVisualChild<Button>();

                    if (buttonFilter != null)
                    {
                        buttonFilter.Opacity = 0.5;
                        pathFilterIcon = VisualTreeHelpers.FindChild<Path>(buttonFilter, "PathFilterIcon");
                        if (pathFilterIcon != null)
                            pathFilterIcon.Data = iconFilter;
                    }


                    if (columnType == typeof(DataGridTextColumn))
                    {
                        var column = (DataGridTextColumn)col;
                     
                        // reset template
                        column.HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate");

                        Type fieldType = null;
                        var fieldProperty = collectionType.GetProperty(((Binding)column.Binding).Path.Path);

                        // get type or underlying type if nullable
                        if (fieldProperty != null)
                            fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;

                        // apply DateFormatString when StringFormat for column is not provided or empty
                        if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                        {
                            if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                column.Binding.StringFormat = DateFormatString;
                        }

                        // culture
                        if (((Binding)column.Binding).ConverterCulture == null)
                            ((Binding)column.Binding).ConverterCulture = Translate.Culture;

                        column.FieldName = ((Binding)column.Binding).Path.Path;
                    }
                    else if (columnType == typeof(DataGridTemplateColumn))
                    {
                        // DataGridTemplateColumn has no culture property
                        var column = (DataGridTemplateColumn)col;

                        // reset template
                        column.HeaderTemplate = (DataTemplate)FindResource("DataGridHeaderTemplate");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.GeneratingCustomColumn : {ex.Message}");
                throw;
            }
        }

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
        /// Reactivate sorting
        /// </summary>
        private void ReactivateSorting()
        {
            if (currentColumn == null) return;

            if (currentColumn is DataGridTextColumn column)
                column.CanUserSort = true;

            if (currentColumn is DataGridTemplateColumn templateColumn)
                templateColumn.CanUserSort = true;
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
            // CanExecute only when the popup is open
            if ((popup?.IsOpen ?? false) == false)
            {
                e.CanExecute = false;
            }
            else
            {
                if (search)
                {
                    // in search, at least one article must be checked
                    e.CanExecute = CurrentFilter?.FieldType == typeof(DateTime)
                    ? CurrentFilter.AnyDateIsChecked()
                    : PopupViewItems.Any(f => f?.IsChecked == true);
                }
                else
                {
                    // on change state, at least one item must be checked
                    // and another must have changed status
                    e.CanExecute = CurrentFilter?.FieldType == typeof(DateTime)
                        ? CurrentFilter.AnyDateChanged()
                        : PopupViewItems.Any(f => f.Changed) && PopupViewItems.Any(f => f?.IsChecked == true);
                }
            }
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

            foreach (var obj in PopupViewItems.ToList()
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
            Debug.WriteLineIf(DebugMode, "PopupClosed");

            var pop = (Popup)sender;

            // free the resources if the popup is closed without filtering
            if (!pending)
            {
                // clear resources
                sourceObjectList = null;
                ItemCollectionView = null;
                ReactivateSorting();
            }

            sizableContentGrid.Width = sizableContentWidth;
            sizableContentGrid.Height = sizableContentHeight;
            Cursor = cursor;

            // fix resize grip: unsubscribe event
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
                return ((DateTime?)item.Content)?.ToString(DateFormatString, Translate.Culture)
                    .IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >=0;

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

            search = !string.IsNullOrEmpty(searchText);

            // apply filter
            ItemCollectionView.Refresh();

            if (CurrentFilter.FieldType != typeof(DateTime) || treeview == null) return;

            // rebuild treeview
            if (string.IsNullOrEmpty(searchText))
            {
                // fill the tree with the elements of the list of the original items
                treeview.ItemsSource = CurrentFilter.BuildTree(sourceObjectList, lastFilter);
            }
            else
            {
                // fill the tree only with the items found by the search
                var items = PopupViewItems.Where(i => i.IsChecked == true)
                    .Select(f => f.Content).ToList();

                // if at least one item is not null, fill in the tree structure
                // otherwise the tree structure contains only the item (select all).
                treeview.ItemsSource = CurrentFilter.BuildTree(items.Any() ? items : null);
            }
        }

        /// <summary>
        ///    Open a pop-up window, Click on the header button
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
                searchTextBox.Focusable = true;
               
                // clear SearchBox button
                var clearSearchBoxBtn = VisualTreeHelpers.FindChild<Button>(popup.Child, "ClearSearchBoxBtn");
                clearSearchBoxBtn.Click += ClearSearchBoxClick;

                // thumb resize grip
                thumb = VisualTreeHelpers.FindChild<Thumb>(sizableContentGrid, "PopupThumb");

                // minimum size of Grid
                sizableContentHeight = 0;
                sizableContentWidth = 0;

                sizableContentGrid.Height = popUpSize.Y;
                sizableContentGrid.MinHeight = popUpSize.Y;

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
                    column.CanUserSort = false;
                    currentColumn = column;
                }

                if (columnType == typeof(DataGridTemplateColumn))
                {
                    var column = (DataGridTemplateColumn)header.Column;
                    fieldName = column.FieldName;
                    column.CanUserSort = false;
                    currentColumn = column;
                }

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                // get type of field
                Type fieldType = null;
                var fieldProperty = collectionType.GetProperty(fieldName);

                // get type or underlying type if nullable
                if (fieldProperty != null)
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

                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                var filterItemList = new List<FilterItem>();

                // get the list of distinct values from the selected column
                // list of raw values of the current column
                await Task.Run(() =>
                {
                    // thank's Stefan Heimel for this contribution
                    UiDispatcher.Invoke(() =>
                    {
                        sourceObjectList = Items.Cast<object>()
                        .Select(x => x.GetType().GetProperty(fieldName)?.GetValue(x, null))
                        .Distinct() // clear duplicate values first
                        .Select(item => item)
                        .ToList();
                    });

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    // displays new (checked) and already filtered (unchecked) items
                    // PreviouslyFilteredItems is a HashSet of objects
                    if (lastFilter == CurrentFilter.FieldName)
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems);

                    // sorting is a slow operation, using ParallelQuery
                    // TODO : AggregateException when user can add row
                    sourceObjectList = sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    // empty item flag
                    var emptyItem = false;

                    // if it exists, remove them from the list
                    if (sourceObjectList.Any(l => string.IsNullOrEmpty(l?.ToString())))
                    {
                        // item = null && items = "" => the empty string is not filtered.
                        // the solution is to add an empty string to the element to filter, see ApplyFilterCommand method
                        emptyItem = true;
                        sourceObjectList.RemoveAll(v => v == null || string.IsNullOrEmpty(v.ToString()));
                    }

                    // add the first element (select all) at the top of list
                    filterItemList = new List<FilterItem>(sourceObjectList.Count + 2)
                    {
                        new FilterItem {Label = Translate.All, IsChecked = true}
                    };

                    // add all items to the filterItemList
                    // filterItemList is used only for search and string list, the dates list is computed by FilterCommon.BuildTree
                    for (var i = 0; i < sourceObjectList.Count; i++)
                    {
                        var item = sourceObjectList[i];
                        var filterItem = new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = item, // raw value
                            Label = item?.ToString(), // Content displayed
                            Level = 1,

                            // check or uncheck if the content of current item exists in the list of previously filtered items
                            // SetState doesn't raise OnpropertyChanged notification
                            SetState = CurrentFilter.PreviouslyFilteredItems?.Contains(item) == false
                        };
                        filterItemList.Add(filterItem);
                    }

                    // add a empty item(if exist) at the bottom of the list
                    if (emptyItem)
                    {
                        sourceObjectList.Insert(sourceObjectList.Count, null);

                        filterItemList.Add(new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = null,
                            Label = Translate.Empty,
                            SetState = CurrentFilter.PreviouslyFilteredItems?.Contains(null) == false
                        });
                    }
                }); // and task

                // the current listbox or treeview
                if (fieldType == typeof(DateTime))
                {
                    treeview = VisualTreeHelpers.FindChild<TreeView>(popup.Child, "PopupTreeview");

                    if (treeview != null)
                    {
                        // fill the treeview with CurrentFilter.BuildTree method
                        // and if it's the last filter, uncheck the items already filtered
                        treeview.ItemsSource =
                            CurrentFilter?.BuildTree(sourceObjectList, lastFilter);
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

                // Set ICollectionView for filtering in the pop-up window
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

                // set focus on searchTextBox
                Keyboard.Focus(searchTextBox);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ShowFilterCommand error : {ex.Message}");
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
                // items already filtered
                var previousFiltered = new List<object>(CurrentFilter.PreviouslyFilteredItems);
                // list of content of items to filter
                var popupItems = PopupViewItems.ToList();

                await Task.Run(() =>
                {
                    // list of content of items not to be filtered
                    List<FilterItem> uncheckedItems;
                    List<FilterItem> checkedItems = null;

                    if (search)
                    {
                        // search items result displayed
                        var searchResult = popupItems;

                        Dispatcher.Invoke(() =>
                        {
                            // remove filter
                            ItemCollectionView.Filter = null;
                        });

                        // popup = all items except searchResult
                        uncheckedItems = PopupViewItems.Except(searchResult).ToList();
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered = previousFiltered.Except(searchResult
                            .Where(c => c.IsChecked == true)
                            .Select(c => c.Content)).ToList();

                        previousFiltered.AddRange(uncheckedItems.Select(c => c.Content));
                    }
                    else
                    {
                        var viewItems = CurrentFilter.FieldType == typeof(DateTime)
                            ? CurrentFilter.GetAllItemsTree().ToList()
                            : popupItems.Where(v => v.Changed).ToList();

                        checkedItems = viewItems.Where(i => i.IsChecked == true).ToList();
                        uncheckedItems = viewItems.Where(i => i.IsChecked == false).ToList();

                        // previous item except unchecked items checked again
                        previousFiltered = previousFiltered.Except(checkedItems.Select(c => c.Content)).ToList();
                        previousFiltered.AddRange(uncheckedItems.Select(c => c.Content));
                    }

                    // two values, null and string.empty for the list of strings
                    if (CurrentFilter.FieldType == typeof(string))
                    {
                        // add string.Empty
                        if (uncheckedItems.Any(v => v.Content == null))
                            previousFiltered.Add(string.Empty);

                        // remove string.Empty
                        if (checkedItems != null && checkedItems.Any(i => i.Content == null))
                            previousFiltered.RemoveAll(item => item?.ToString() == string.Empty);
                    }

                    // fill the PreviouslyFilteredItems HashSet with unchecked items
                    CurrentFilter.PreviouslyFilteredItems = new HashSet<object>(previousFiltered,
                        EqualityComparer<object>.Default);

                    // add a filter if it is not already added previously
                    if (!CurrentFilter.IsFiltered)
                        CurrentFilter.AddFilter(criteria);

                    // add current filter to GlobalFilterList
                    if (GlobalFilterList.All(f => f.FieldName != CurrentFilter.FieldName))
                        GlobalFilterList.Add(CurrentFilter);

                    // set the current field name as the last filter name
                    lastFilter = CurrentFilter.FieldName;
                });

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
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                ReactivateSorting();
                pending = false;
                ResetCursor();
                ElapsedTime = elased.Add(DateTime.Now - start);

                Debug.WriteLineIf(DebugMode, $"Elapsed time : {ElapsedTime:mm\\:ss\\.ff}");
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
                popup.HorizontalOffset = 0d;
                popup.VerticalOffset = -1d;
                popup.Placement = PlacementMode.Bottom;

                // get the host window of the datagrid
                // thank's Stefan Heimel for this contribution
                var hostingWindow = Window.GetWindow(this);

                if (hostingWindow != null)
                {
                    const double border = 1d;

                    // get the ContentPresenter from the hostingWindow
                    var contentPresenter = VisualTreeHelpers.FindChild<ContentPresenter>(hostingWindow);

                    var hostSize = new Point
                    {
                        X = contentPresenter.ActualWidth,
                        Y = contentPresenter.ActualHeight
                    };

                    // get the X, Y position of the header
                    var headerContentOrigin = header.TransformToVisual(contentPresenter).Transform(new Point(0, 0));
                    var headerDataGridOrigin = header.TransformToVisual(this).Transform(new Point(0, 0));

                    var headerSize = new Point { X = header.ActualWidth, Y = header.ActualHeight };
                    var offset = popUpSize.X - headerSize.X + border;

                    // the popup must stay in the DataGrid, move it to the left of the header,
                    // because it overflows on the right.
                    if (headerDataGridOrigin.X + headerSize.X > popUpSize.X)
                    {
                        popup.HorizontalOffset -= offset;
                    }

                    // delta for max size popup
                    var delta = new Point
                    {
                        X = hostSize.X - (headerContentOrigin.X + headerSize.X),
                        Y = hostSize.Y - (headerContentOrigin.Y + headerSize.Y + popUpSize.Y)
                    };

                    // max size
                    grid.MaxWidth = popUpSize.X + delta.X - border;
                    grid.MaxHeight = popUpSize.Y + delta.Y - border;

                    // remove offset
                    if (popup.HorizontalOffset == 0)
                        grid.MaxWidth -= offset;

                    // the height of popup is too large, reduce it, because it overflows down.
                    if (delta.Y <= 0d)
                    {
                        grid.MaxHeight = popUpSize.Y - Math.Abs(delta.Y) - border;
                        grid.Height = grid.MaxHeight;
                        grid.MinHeight = grid.MaxHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.PopupPlacement error : {ex.Message}");
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

        /// <summary>
        /// FilterDataGrid Dictionary
        /// </summary>
        public FilterDataGridDictionary()
        {
            InitializeComponent();
        }

        #endregion Public Constructors
    }
}