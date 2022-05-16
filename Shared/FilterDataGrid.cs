// Author     : Gilles Macabies
// Solution   : FIlterConsole
// Projet     : FilterDataGrid
// File       : FilterDataGrid.cs
// Created    : 02/05/2022
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantArgumentDefaultValue

// https://michaelscodingspot.com/find-fix-and-avoid-memory-leaks-in-c-net-8-best-practices/

namespace FilterDataGrid
{
    /// <summary>
    ///     Implementation of Datagrid
    /// </summary>
    public sealed class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region Public Fields

        public static readonly ICommand ApplyFilter = new RoutedCommand();

        public static readonly ICommand CancelFilter = new RoutedCommand();

        public static readonly ICommand ClearSearchBox = new RoutedCommand();

        /// <summary>
        ///     date format displayed
        /// </summary>
        public static readonly DependencyProperty DateFormatStringProperty =
            DependencyProperty.Register("DateFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata("d"));

        /// <summary>
        ///     Excluded Fields on AutoColumn
        /// </summary>
        public static readonly DependencyProperty ExcludeFieldsProperty =
            DependencyProperty.Register("ExcludeFields",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(""));

        /// <summary>
        ///     Language displayed
        /// </summary>
        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local),
                typeof(FilterDataGrid),
                new PropertyMetadata(Local.English));

        public static readonly ICommand IsChecked = new RoutedCommand();

        public static readonly ICommand RemoveFilter = new RoutedCommand();

        /// <summary>
        ///     Show elapsed time in status bar
        /// </summary>
        public static readonly DependencyProperty ShowElapsedTimeProperty =
            DependencyProperty.Register("ShowElapsedTime",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        public static readonly ICommand ShowFilter = new RoutedCommand();

        /// <summary>
        ///     Show Rows Count
        /// </summary>
        public static readonly DependencyProperty ShowRowsCountProperty =
            DependencyProperty.Register("ShowRowsCount",
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

        #endregion Public Fields

        #region Private Fields

        private const bool IsDebugModeOn = false;

        /// <summary>
        ///     Handle Mousedown, contribution : WORDIBOI
        /// </summary>
        private readonly MouseButtonEventHandler onMousedown = (o, eArgs) => { eArgs.Handled = true; };

        private readonly StringComparison ordinalIgnoreCase = StringComparison.OrdinalIgnoreCase;

        private Button button;

        private Type collectionType;

        private Cursor cursor;

        private TimeSpan elased;

        private List<string> excludedFields;

        private string fieldName;

        private Type fieldType;

        private FilterManager filterManager;

        private List<FilterItem> listBoxItems;
        private double minHeight;

        private double minWidth;

        private bool pending;

        private Popup popup;

        private Point popUpSize;

        private bool search;

        private int searchLength;

        private string searchText;

        private TextBox searchTextBox;

        private Grid sizableContentGrid;

        private double sizableContentHeight;

        private double sizableContentWidth;

        private bool startsWith;

        private Stopwatch stopWatchFilter = new Stopwatch();

        private Thumb thumb;

        private List<FilterItemDate> treeview;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        ///     FilterDataGrid constructor
        /// </summary>
        public FilterDataGrid()
        {
            DefaultStyleKey = typeof(FilterDataGrid);

            Debug.WriteLineIf(IsDebugModeOn, "Constructor");

            // load resources
            var resourcesDico = new ResourceDictionary
            {
                Source = new Uri("/FilterDataGrid;component/Themes/FilterDataGrid.xaml",
                    UriKind.RelativeOrAbsolute)
            };

            Resources.MergedDictionaries.Add(resourcesDico);

            // initial popup size
            popUpSize = new Point
            {
                X = (double)TryFindResource("PopupWidth"),
                Y = (double)TryFindResource("PopupHeight")
            };

            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand, CanShowFilter));
            CommandBindings.Add(new CommandBinding(ApplyFilter, ApplyFilterCommand, CanApplyFilter)); // Ok
            CommandBindings.Add(new CommandBinding(CancelFilter, CancelFilterCommand));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterCommand, CanRemoveFilter));
            CommandBindings.Add(new CommandBinding(IsChecked, CheckedAllCommand));
            CommandBindings.Add(new CommandBinding(ClearSearchBox, ClearSearchBoxClick));
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Sorted;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        ///     Date format displayed
        /// </summary>
        public string DateFormatString
        {
            get => (string)GetValue(DateFormatStringProperty);
            set => SetValue(DateFormatStringProperty, value);
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
        ///     Excluded Fileds
        /// </summary>
        public string ExcludeFields
        {
            get => (string)GetValue(ExcludeFieldsProperty);
            set => SetValue(ExcludeFieldsProperty, value);
        }

        /// <summary>
        ///     Language
        /// </summary>
        public Local FilterLanguage
        {
            get => (Local)GetValue(FilterLanguageProperty);
            set => SetValue(FilterLanguageProperty, value);
        }

        /// <summary>
        ///     Display items count
        /// </summary>
        public int ItemsSourceCount { get; set; }

        public List<FilterItem> ListBoxItems
        {
            get => listBoxItems ?? new List<FilterItem>();
            set
            {
                listBoxItems = value;
                OnPropertyChanged(nameof(ListBoxItems));
            }
        }

        /// <summary>
        ///     Row header size when ShowRowsCount is true
        /// </summary>
        public double RowHeaderSize { get; set; }

        /// <summary>
        ///     Show elapsed time in status bar
        /// </summary>
        public bool ShowElapsedTime
        {
            get => (bool)GetValue(ShowElapsedTimeProperty);
            set => SetValue(ShowElapsedTimeProperty, value);
        }

        /// <summary>
        ///     Show rows count
        /// </summary>
        public bool ShowRowsCount
        {
            get => (bool)GetValue(ShowRowsCountProperty);
            set => SetValue(ShowRowsCountProperty, value);
        }

        /// <summary>
        ///     Show status bar
        /// </summary>
        public bool ShowStatusBar
        {
            get => (bool)GetValue(ShowStatusBarProperty);
            set => SetValue(ShowStatusBarProperty, value);
        }

        /// <summary>
        ///     String begins with the specified character. Used in popup searchBox
        /// </summary>
        public bool StartsWith
        {
            get => startsWith;
            set
            {
                startsWith = value;
                OnPropertyChanged();

                // refresh filter
                if (!string.IsNullOrEmpty(searchText)) ItemCollectionView.Refresh();
            }
        }

        /// <summary>
        ///     Instance of Loc
        /// </summary>
        public Loc Translate { get; private set; }

        public List<FilterItemDate> TreeviewItems
        {
            get => treeview ?? new List<FilterItemDate>();
            set
            {
                treeview = value;
                OnPropertyChanged(nameof(TreeviewItems));
            }
        }

        #endregion Public Properties

        #region Private Properties

        private IEnumerable<FilterItem> CommonItemsView =>
            ItemCollectionView?.OfType<FilterItem>().Skip(1) ?? new List<FilterItem>();

        private IEnumerable<FilterItem> CommonSourceItemsView =>
            ItemCollectionView?.SourceCollection.OfType<FilterItem>().Skip(1) ?? new List<FilterItem>();

        private ICollectionView DatagridCollectionView { get; set; }

        private ICollectionView ItemCollectionView { get; set; }

        #endregion Private Properties

        #region Protected Methods

        /// <summary>
        ///     Auto generated column, set templateHeader
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            Debug.WriteLineIf(IsDebugModeOn, "OnAutoGeneratingColumn");

            base.OnAutoGeneratingColumn(e);

            try
            {
                if (e.Column.GetType() != typeof(System.Windows.Controls.DataGridTextColumn)) return;

                var column = new DataGridTextColumn
                {
                    Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture /* StringFormat */ },
                    FieldName = e.PropertyName,
                    Header = e.Column.Header.ToString(),
                    IsColumnFiltered = false
                };

                // get type
                fieldType = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                // apply the format string provided
                if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                    column.Binding.StringFormat = DateFormatString;

                // add DataGridHeaderTemplate template if not excluded
                if (excludedFields?.FindIndex(c =>
                        string.Equals(c, e.PropertyName, StringComparison.CurrentCultureIgnoreCase)) == -1)
                {
                    column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");
                    column.IsColumnFiltered = true;
                }

                e.Column = column;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnAutoGeneratingColumn : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            // call order : Constructor => OnInitialized => OnItemsSourceChanged

            Debug.WriteLineIf(IsDebugModeOn, "OnInitialized");

            base.OnInitialized(e);

            try
            {
                // FilterLanguage : default : 0 (english)
                Translate = new Loc { Language = FilterLanguage };

                // Show row count
                RowHeaderWidth = ShowRowsCount ? RowHeaderWidth > 0 ? RowHeaderWidth : double.NaN : 0;

                // fill excluded Fields list with values
                if (AutoGenerateColumns)
                    excludedFields = ExcludeFields.Split(',').Select(p => p.Trim()).ToList();

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
        ///     The source of the Datagrid items has been changed (refresh or on loading)
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Debug.WriteLineIf(IsDebugModeOn, "OnItemsSourceChanged");

            base.OnItemsSourceChanged(oldValue, newValue);

            try
            {
                ElapsedTime = new TimeSpan(0, 0, 0);
                // initialize FilterManager
                filterManager = new FilterManager(Items.Count);

                //Debug.WriteLine($"FilterDataGrid : {((IList)newValue).Count}");

                //if (newValue == null || ((IList)newValue).Count == 0) return;

                // clear ICollectionView
                if (DatagridCollectionView != null)
                {
                    DatagridCollectionView = CollectionViewSource.GetDefaultView(new List());
                    ItemCollectionView = CollectionViewSource.GetDefaultView(new List());
                }

                DatagridCollectionView = CollectionViewSource.GetDefaultView(ItemsSource);

                // set Filter, contribution : STEFAN HEIMEL
                if (DatagridCollectionView.CanFilter) DatagridCollectionView.Filter = Filter;

                ItemsSourceCount = Items.Count;
                OnPropertyChanged(nameof(ItemsSourceCount));

                // Calculate row header width
                if (ShowRowsCount)
                {
                    var txt = new TextBlock
                    {
                        Text = ItemsSourceCount.ToString(),
                        FontSize = FontSize,
                        FontFamily = FontFamily,
                        Margin = new Thickness(2.0)
                    };
                    txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    RowHeaderSize = Math.Ceiling(txt.DesiredSize.Width);
                    OnPropertyChanged(nameof(RowHeaderSize));
                }

                // get collection type
                if (ItemsSourceCount > 0)
                    // contribution : APFLKUACHA
                    collectionType = ItemsSource is ICollectionView collectionView
                        ? collectionView.SourceCollection?.GetType().GenericTypeArguments.FirstOrDefault()
                        : ItemsSource?.GetType().GenericTypeArguments.FirstOrDefault();

                // scroll to top on reload collection
                if (oldValue != null)
                {
                    var scrollViewer = GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
                    scrollViewer?.ScrollToTop();
                }

                // generating custom columns
                if (!AutoGenerateColumns && collectionType != null) GeneratingCustomsColumn();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnItemsSourceChanged : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Adding Rows count
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
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
            Sorted?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        ///     Click OK Button when Popup is Open, apply filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(IsDebugModeOn, "\r\nApplyFilterCommand");

            pending = true;
            popup.IsOpen = false; // raise PopupClosed event

            // set cursor wait
            Mouse.OverrideCursor = Cursors.Wait;

            stopWatchFilter.Start();

            try
            {
                var currentFilter = filterManager.CurrentFilter;

                if (currentFilter == null) return;

                // initialize
                var changed = new List<FilterItem>();

                await Task.Run(() =>
                {
                    if (search)
                    {
                        var searchResult = CommonItemsView.Where(c => c.IsChecked && !c.IsChanged).ToList();
                        changed = CommonSourceItemsView.Except(searchResult).ToList();

                        // change the state( IsChecked = false) in items that have not changed (state)
                        foreach (var item in changed.Where(c => !c.IsChanged && c.IsChecked))
                            item.Initialize = false;  // initialize don't raise OnPropertyChanged
                    }
                    else
                    {
                        changed = CommonItemsView.Where(c => c.IsChanged).ToList();
                    }
                });

                if (changed.Any())
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < changed.Count; i++)
                    {
                        var change = changed[i];
                        var groupIndex = change.GroupIndex;

                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (var j = 0; j < groupIndex.Length; j++)
                        {
                            var index = groupIndex[j];

                            // if unchecked(false) => PreviousItems = true
                            // if checked(true)    => PreviousItems = false

                            // change state in StackItems, PreviousItems, Stock BitArray
                            filterManager.StackItems[index] = change.IsChecked;
                            currentFilter.PreviousItems[index] = !change.IsChecked;
                            // filterManager.Stock[index] = !change.IsChecked;
                        }
                    }

                    // enqueue => dequeue, then requeue, currentFilter.IsFiltered is set to true
                    filterManager.Enqueue();

                    // check if all items are checked
                    var isAllChecked = CommonItemsView.Any(c => !c.IsChecked) != true;

                    // if current filter don't have any previous, remove from Queue
                    if (isAllChecked && !search)
                    {
                        Debug.WriteLineIf(IsDebugModeOn,
                            $"Not Any true => Dequeue and Remove {currentFilter.FieldName} filter");

                        RemoveCurrentFilter();

                        // warning : it's not efficient
                        return;
                    }

                    filterManager.PrintState($"Apply Filter : {currentFilter.FieldName}", IsDebugModeOn);
                }

                if (DatagridCollectionView.Filter == null) DatagridCollectionView.Filter = Filter;
                else DatagridCollectionView.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // set button icon
                FilterState.SetIsFiltered(button, filterManager.CurrentFilter?.IsFiltered ?? false);

                // clear collection
                ItemCollectionView = CollectionViewSource.GetDefaultView(new List());

                // warning : it's necessary ?
                filterManager.CurrentFilter = null;

                // clear search text (!important)
                searchText = string.Empty;
                search = false;
                pending = false;

                TreeviewItems = new List<FilterItemDate>();
                ListBoxItems = new List<FilterItem>();

                ResetCursor();
                ReactivateSorting();

                stopWatchFilter.Stop();
                ElapsedTime = stopWatchFilter.Elapsed;

                Debug.WriteLineIf(IsDebugModeOn, $"Elapsed time : {ElapsedTime:mm\\:ss\\.ff}");
            }
        }

        private List<FilterItemDate> BuildTreeView(IEnumerable<FilterItem> dates)
        {
            var watch = new Stopwatch();
            watch.Start();

            var tree = new List<FilterItemDate>
            {
                new FilterItemDate
                {
                    Label = "(Select all)", Content = null, Level = 0, Initialize = true
                }
            };

            // IMPORTANT :  when dates collection is null, return the tree with (Select all) item only
            if (dates == null) return tree;

            var dateTimes = dates.ToList();

            try
            {
                foreach (var y in dateTimes.Where(c => c.Content != null)
                             .Select(d => new
                             {
                                 Date = (DateTime)d.Content,
                                 d.IsChecked,
                                 d.IsPrevious,
                                 d.GroupIndex,
                                 Item = d
                             })
                             .OrderBy(o => o.Date.Year)
                             .GroupBy(g => g.Date.Year)
                             .Select(year => new FilterItemDate
                             {
                                 Level = 1,
                                 Label = $"{year.FirstOrDefault()?.Date:yyyy}",
                                 // + $" - {string.Join(" ", year.FirstOrDefault()?.GroupIndex ?? new List<int>())}",
                                 GroupIndex = Array.Empty<int>(),
                                 Content = year.Key,
                                 Initialize = true,
                                 IsPrevious = year.FirstOrDefault()?.IsPrevious ?? false,
                                 Children = year.GroupBy(date => date.Date.Month)
                                     .Select(month => new FilterItemDate
                                     {
                                         Level = 2,
                                         Label = $"{month.FirstOrDefault()?.Date:MMMM}",
                                         // + $"- {string.Join(" ", month.FirstOrDefault()?.GroupIndex ?? new List<int>())}",
                                         GroupIndex = Array.Empty<int>(),
                                         Content = month.Key,
                                         Initialize = true,
                                         IsPrevious = month.FirstOrDefault()?.IsPrevious ?? false,
                                         Children = month.GroupBy(date => date.Date.Day)
                                             .Select(day => new FilterItemDate
                                             {
                                                 Level = 3,
                                                 Label = $"{day.FirstOrDefault()?.Date:dd}",
                                                 // +$"- {string.Join(" , ", day.FirstOrDefault()?.GroupIndex ?? new List<int>())}",
                                                 GroupIndex = day.FirstOrDefault()?.GroupIndex,
                                                 Content = day.Key,
                                                 Initialize = true,
                                                 IsPrevious = !day.FirstOrDefault()?.IsChecked ?? true,
                                                 Children = new List<FilterItemDate>(),
                                                 Item = day.FirstOrDefault()?.Item
                                             }).ToList()
                                     }).ToList()
                             }))
                {
                    y.Children.ForEach(m =>
                    {
                        // month
                        m.Parent = y;

                        m.Children.ForEach(d =>
                        {
                            // day
                            d.Parent = m;

                            // if previous is true, ischecked is false
                            // to avoid (ischanged == true), initialize to false
                            if (!d.IsPrevious) return;

                            d.IsChecked = false;
                            // stay ischanged == false
                            d.Initialize = d.IsChecked;
                        });
                    });
                    tree.Add(y);
                }

                // Debug.WriteLine($"*** buildTree : {tree.Count} ***");

                // last empty item if exist in collection
                if (dateTimes.Any(d => d.Content == null))
                {
                    var empty = dateTimes.FirstOrDefault(c => c.Content == null);

                    if (empty != null)
                        tree.Add(
                            new FilterItemDate
                            {
                                Level = -1,
                                Label = Translate.Empty,
                                //Label = $"(Empty) - {string.Join(" , ", empty.GroupIndex ?? new List<int>())}",
                                GroupIndex = empty.GroupIndex,
                                Content = null,
                                Initialize = empty.IsChecked,
                                IsPrevious = empty.IsPrevious,
                                Children = new List<FilterItemDate>(),
                                Item = empty
                            }
                        );
                }

                // only the first element of the tree (select all) has a tree reference, because it selects all or none,
                // the iteration is performed on the tree by calling the SetIsChecked method
                tree.First().Tree = tree;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }

            watch.Display($"\r\nBuildTreeView {filterManager.CurrentFilter.FieldName}", IsDebugModeOn);

            return tree;
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
                e.CanExecute = search
                    ? CommonItemsView.Any()
                    : CommonItemsView.Any(c => c.IsChecked) && CommonItemsView.Any(c => c.IsChanged);
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
            e.CanExecute = filterManager.CurrentFilter?.IsFiltered ?? false;
        }

        /// <summary>
        ///     Can show filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanShowFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            // e.CanExecute = CollectionDataGridView?.CanFilter == true && (!popup?.IsOpen ?? true) && !pending;

            e.CanExecute = (!popup?.IsOpen ?? true) && !pending;
        }

        /// <summary>
        ///     Check/uncheck all item when the action is (select all)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e == null) return;
            var item = (FilterItem)e.Parameter;

            if (item.Level == 0)
                foreach (var element in CommonItemsView.Where(f => f.IsChecked != item.IsChecked))
                {
                    // exclude event
                    element.IsChecked = item.IsChecked;
                }
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

        ///// <summary>
        /////     Datagrid filter
        ///// </summary>
        private bool Filter(object o)
        {
            return filterManager.Current();
        }

        /// <summary>
        ///     Generate custom columns that can be filtered
        /// </summary>
        private void GeneratingCustomsColumn()
        {
            Debug.WriteLineIf(IsDebugModeOn, "GeneratingCustomColumn");

            try
            {
                // get the columns that can be filtered
                var columns = Columns
                    .Where(c => (c is DataGridTextColumn dtx && dtx.IsColumnFiltered) ||
                                (c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered))
                    .Select(c => c)
                    .ToList();

                // set header template
                foreach (var col in columns)
                {
                    var columnType = col.GetType();

                    if (col.HeaderTemplate != null)
                    {
                        // reset filter Button
                        var buttonFilter = VisualTreeHelpers.GetHeader(col, this)
                            ?.FindVisualChild<Button>("FilterButton");
                        if (buttonFilter != null) FilterState.SetIsFiltered(buttonFilter, false);
                    }
                    else
                    {
                        if (columnType == typeof(DataGridTextColumn))
                        {
                            var column = (DataGridTextColumn)col;

                            // template
                            column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");

                            fieldType = null;
                            var fieldProperty = collectionType.GetProperty(((Binding)column.Binding).Path.Path);

                            // get type or underlying type if nullable
                            if (fieldProperty != null)
                                fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                            fieldProperty.PropertyType;

                            // apply DateFormatString when StringFormat for column is not provided or empty
                            if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                                if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                    column.Binding.StringFormat = DateFormatString;

                            // culture
                            if (((Binding)column.Binding).ConverterCulture == null)
                                ((Binding)column.Binding).ConverterCulture = Translate.Culture;

                            column.FieldName = ((Binding)column.Binding).Path.Path;
                        }
                        else if (columnType == typeof(DataGridTemplateColumn))
                        {
                            // DataGridTemplateColumn has no culture property
                            var column = (DataGridTemplateColumn)col;

                            // template
                            column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");
                        }
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
        /// Generate list of current field values using for loop
        /// </summary>
        /// <param name="fieldProperty"></param>
        /// <returns></returns>
        private List<FilterItem> GetColumnValuesForLoop(PropertyInfo fieldProperty)
        {
            var watch = new Stopwatch();
            watch.Start();

            if (!(ItemsSource is IList items)) return null;

            var currentFilter = filterManager.CurrentFilter;
            var isLastFilter = filterManager.LastFilter?.FieldName == fieldProperty.Name;
            var dico = new Dictionary<object, List<GroupIndexState>>();
            var isDate = fieldType == typeof(DateTime);
            List<FilterItem> resultList;

            try
            {
                for (var index = 0; index < items.Count; index++)
                {
                    if (!filterManager.StackItems[index] &&
                        (!isLastFilter || !currentFilter.PreviousItems[index])) continue;

                    var entry = isDate
                        ? ((DateTime?)fieldProperty.GetValue(items[index], null))?.Date
                        : fieldProperty.GetValue(items[index], null);

                    var isChecked = filterManager.StackItems[index];
                    var isprevious = currentFilter.PreviousItems[index];

                    var key = entry ?? string.Empty;

                    if (dico.ContainsKey(key))
                    {
                        var current = dico[key][0];

                        // if both are true, checked has priority
                        current.IsChecked = isChecked || current.IsChecked;
                        current.IsPrevious = !current.IsChecked && current.IsPrevious;

                        if (isChecked)
                            current.CheckedIndex.Add(index);
                        else
                            current.PreviousIndex.Add(index);

                        dico[key][0] = current;
                    }
                    else
                    {
                        // initialize
                        var groupIndexState = new GroupIndexState
                        {
                            // cannot have the same state at the same time
                            IsChecked = isChecked,
                            IsPrevious = isprevious,
                            CheckedIndex = new List<int>(),
                            PreviousIndex = new List<int>(),
                            IsNull = key.Equals(string.Empty),
                            Content = entry
                        };

                        if (isChecked)
                            groupIndexState.CheckedIndex.Add(index);
                        else
                            groupIndexState.PreviousIndex.Add(index);

                        dico.Add(key, new List<GroupIndexState> { groupIndexState });
                    }
                }

                resultList = dico //.AsParallel().OrderBy(x => x.Key.ToString())
                    .Select((c, i) => new FilterItem
                    {
                        GroupIndex = c.Value[0].IsChecked
                            ? c.Value[0].CheckedIndex.ToArray()
                            : c.Value[0].PreviousIndex.ToArray(),
                        Initialize = c.Value[0].IsChecked,
                        IsPrevious = c.Value[0].IsPrevious,
                        Content = c.Value[0].IsNull ? null : c.Value[0].Content,
                        FieldType = fieldType,
                        Level = 1
                    })
                    .AsParallel().OrderBy(x => x.Content)
                    .ToList();

                watch.Display("GetColumnValues For Loop Final List", IsDebugModeOn);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.GetColumnValuesForLoop : {ex.Message}");
                throw;
            }

            return resultList;
        }

        // ReSharper disable once UnusedMember.Local
        /// <summary>
        ///  Generate list of current field values using while loop
        /// </summary>
        /// <param name="fieldProperty"></param>
        /// <returns></returns>
        private List<FilterItem> GetColumnValuesWhileLoop(PropertyInfo fieldProperty)
        {
            var watch = new Stopwatch();
            watch.Start();

            var currentFilter = filterManager.CurrentFilter;

            // filterManager.LastFilter : last Filter entered in queue
            var isLastFilter = filterManager.LastFilter?.FieldName == fieldProperty.Name;

            List<FilterItem> resultList;
            var dico = new Dictionary<object, List<GroupIndexState>>();
            var enumerator = ItemsSource.GetEnumerator();
            var index = 0;
            var isDate = fieldType == typeof(DateTime);

            try
            {
                while (enumerator.MoveNext())
                {
                    if (filterManager.StackItems[index] || (isLastFilter && currentFilter.PreviousItems[index]))
                    {
                        var entry = isDate
                             ? ((DateTime?)fieldProperty.GetValue(enumerator.Current, null))?.Date
                             : fieldProperty.GetValue(enumerator.Current, null);

                        var isChecked = filterManager.StackItems[index];
                        var isprevious = currentFilter.PreviousItems[index];

                        var key = entry ?? string.Empty;

                        if (dico.ContainsKey(key))
                        {
                            var current = dico[key][0];

                            // if both are true, checked has priority
                            current.IsChecked = isChecked || current.IsChecked;
                            current.IsPrevious = !current.IsChecked && current.IsPrevious;

                            if (isChecked)
                                current.CheckedIndex.Add(index);
                            else
                                current.PreviousIndex.Add(index);

                            dico[key][0] = current;
                        }
                        else
                        {
                            // initialize
                            var groupIndexState = new GroupIndexState
                            {
                                // cannot have the same state at the same time
                                IsChecked = isChecked,
                                IsPrevious = isprevious,
                                CheckedIndex = new List<int>(),
                                PreviousIndex = new List<int>(),
                                IsNull = key.Equals(string.Empty),
                                Content = entry
                            };

                            if (isChecked)
                                groupIndexState.CheckedIndex.Add(index);
                            else
                                groupIndexState.PreviousIndex.Add(index);

                            dico.Add(key, new List<GroupIndexState> { groupIndexState });
                        }
                    }

                    index++;
                }

                resultList = dico.AsParallel().OrderBy(x => x.Key.ToString())
                            .Select((c, i) => new FilterItem
                            {
                                GroupIndex = c.Value[0].IsChecked
                                    ? c.Value[0].CheckedIndex.ToArray()
                                    : c.Value[0].PreviousIndex.ToArray(),
                                Initialize = c.Value[0].IsChecked,
                                IsPrevious = c.Value[0].IsPrevious,
                                Content = c.Value[0].IsNull ? null : c.Value[0].Content,
                                FieldType = fieldType,
                                Level = 1
                            })
                            .ToList();

                watch.Display("GetColumnValues While Loop Final List", IsDebugModeOn);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.GetColumnValuesWhileLoop : {ex.Message}");
                throw;
            }

            return resultList;
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
        ///     Reset the cursor at the end of the sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSorted(object sender, EventArgs e)
        {
            ResetCursor();
        }

        /// <summary>
        ///     Reset the size of popup to original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupClosed(object sender, EventArgs e)
        {
            Debug.WriteLineIf(IsDebugModeOn, "PopupClosed");

            var pop = (Popup)sender;

            // free the resources if the popup is closed without filtering
            if (!pending)
            {
                // clear resources
                ItemCollectionView = CollectionViewSource.GetDefaultView(new List());

                TreeviewItems = new List<FilterItemDate>();
                ListBoxItems = new List<FilterItem>();
                filterManager.CurrentFilter = null;

                // clear search text (!important)
                searchText = string.Empty;
                search = false;
                ReactivateSorting();
            }

            // unsubscribe from event and re-enable datagrid
            pop.MouseDown -= onMousedown;
            pop.Closed -= PopupClosed;
            thumb.DragCompleted -= OnResizeThumbDragCompleted;
            thumb.DragDelta -= OnResizeThumbDragDelta;
            thumb.DragStarted -= OnResizeThumbDragStarted;
            searchTextBox.TextChanged -= SearchTextBoxOnTextChanged;

            sizableContentGrid.Width = sizableContentWidth;
            sizableContentGrid.Height = sizableContentHeight;
            Cursor = cursor;

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();

            // re-enable datagrid
            IsEnabled = true;
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

                // get the host window of the datagrid, contribution : STEFAN HEIMEL
                var hostingWindow = Window.GetWindow(this);

                if (hostingWindow != null)
                {
                    // greater than or equal to 0.0
                    double MaxSize(double size)
                    {
                        return size >= 0.0d ? size : 0.0d;
                    }

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

                    // the popup must stay in the DataGrid, move it to the left of the header, because it overflows on the right.
                    if (headerDataGridOrigin.X + headerSize.X > popUpSize.X) popup.HorizontalOffset -= offset;

                    // delta for max size popup
                    var delta = new Point
                    {
                        X = hostSize.X - (headerContentOrigin.X + headerSize.X),
                        Y = hostSize.Y - (headerContentOrigin.Y + headerSize.Y + popUpSize.Y)
                    };

                    // max size
                    grid.MaxWidth = MaxSize(popUpSize.X + delta.X - border);
                    grid.MaxHeight = MaxSize(popUpSize.Y + delta.Y - border);

                    // remove offset
                    if (popup.HorizontalOffset == 0)
                        grid.MaxWidth = MaxSize(grid.MaxWidth -= offset);

                    // the height of popup is too large, reduce it, because it overflows down.
                    if (delta.Y <= 0d)
                    {
                        grid.MaxHeight = MaxSize(popUpSize.Y - Math.Abs(delta.Y) - border);
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

        /// <summary>
        ///     Reactivate sorting
        /// </summary>
        private void ReactivateSorting()
        {
            Sorted += OnSorted;
        }

        /// <summary>
        ///     Remove current filter
        /// </summary>
        private void RemoveCurrentFilter()
        {
            Debug.WriteLineIf(IsDebugModeOn, $"RemoveCurrentFilter :{filterManager.CurrentFilter?.FieldName}");

            if (filterManager.CurrentFilter == null) return;

            popup.IsOpen = false; // raise PopupClosed event

            stopWatchFilter = Stopwatch.StartNew();
            ElapsedTime = new TimeSpan(0, 0, 0);

            Mouse.OverrideCursor = Cursors.Wait;

            var watch = new Stopwatch();
            watch.Start();

            var current = filterManager.CurrentFilter;

            if (current == null) return;

            // button icon reset
            FilterState.SetIsFiltered(button, false);

            // remove current filter to Queue AND the previous items
            // important because else the queue count is false
            filterManager.Dequeue(current);

            // this is the last filter after removing all previous ones
            if (filterManager.IsLast)
            {
                var last = filterManager.LastFilter;

                var bits = new bool[filterManager.StackItems.Count];

                filterManager.StackItems.CopyTo(bits, 0);

                last.PreviousItems = new BitArray(bits);
                last.PreviousItems.Not(); // reverse state

                filterManager.HasPrecedent = false;
            }

            if (filterManager.Queue.Count == 0)
                // reset stack and stock
                filterManager.StackItems.SetAll(true);
            //filterManager.Stock.SetAll(false);

            DatagridCollectionView?.Refresh();
            ResetCursor();

            stopWatchFilter.Stop();
            ElapsedTime = stopWatchFilter.Elapsed;
            watch.Display($"RemoveFilter {filterManager.CurrentFilter?.FieldName}", IsDebugModeOn);
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
        ///     Reset cursor
        /// </summary>
        private async void ResetCursor()
        {
            // reset cursor
            await Dispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = null; }),
                DispatcherPriority.ContextIdle);
        }

        /// <summary>
        ///     Filter current list in popup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            var item = (FilterItem)obj;

            if (string.IsNullOrEmpty(searchText) || item == null || item.Level == 0) return true;

            bool result;

            // Contains
            if (!StartsWith)
            {
                result = item.FieldType == typeof(DateTime)
                    ? ((DateTime?)item.Content)?.ToString(DateFormatString, Translate.Culture)
                    .IndexOf(searchText, ordinalIgnoreCase) >= 0
                    : item.Content?.ToString()?.IndexOf(searchText, ordinalIgnoreCase) >= 0;

                //Debug.WriteLine($"item : {item?.Content,-10} {item?.Level,-4} {result,-8} StartsWith :{StartsWith}");
                return result;
            }

            // StartsWith preserve RangeOverflow
            if (searchLength > item.ContentLength) return false;

            result = item.FieldType == typeof(DateTime)
                ? ((DateTime?)item.Content)?.ToString(DateFormatString, Translate.Culture)
                .IndexOf(searchText, 0, searchLength, ordinalIgnoreCase) >= 0
                : item.Content?.ToString()?.IndexOf(searchText, 0, searchLength, ordinalIgnoreCase) >=
                  0;

            //Debug.WriteLine($"item : {item?.Content, -10} {item?.Level, -4} {result, -8} StartsWith :{StartsWith}");

            return result;
        }

        /// <summary>
        ///     Search TextBox Text Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            // fix TextChanged event fires twice I did not find another solution
            if (textBox == null || textBox.Text == searchText || ItemCollectionView == null) return;

            searchText = textBox.Text;

            searchLength = searchText.Length;

            search = !string.IsNullOrEmpty(searchText);

            // apply filter
            ItemCollectionView.Refresh();

            // only for treeview
            if (filterManager.CurrentFilter.FieldType != typeof(DateTime) || TreeviewItems == null) return;

            // searchText is empty => rebuild treeview
            if (string.IsNullOrEmpty(searchText))
            {
                // populate the tree with the original source items
                TreeviewItems = BuildTreeView(CommonSourceItemsView);
            }
            else
            {
                // populate the tree only with items found by the search when content is not null
                // .Where(c => c.ContentLength > 0)
                var items = CommonItemsView.ToList();
                TreeviewItems = BuildTreeView(items.Any() ? items : null);
            }
        }

        /// <summary>
        ///     Open a pop-up window, Click on the header button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(IsDebugModeOn, "\r\nShowFilterCommand");

            // reset previous elapsed time
            stopWatchFilter = Stopwatch.StartNew();

            try
            {
                // filter button
                button = (Button)e.OriginalSource;

                if (Items.Count == 0 || button == null) return;

                // contribution : OTTOSSON
                // for the moment this functionality is not tested, I do not know if it can cause unexpected effects
                _ = CommitEdit(DataGridEditingUnit.Row, true);

                // navigate up to the current header and get column type
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                var columnType = header.Column.GetType();

                // then down to the current popup
                popup = VisualTreeHelpers.FindChild<Popup>(header, "FilterPopup");

                if (popup == null) return;

                // popup handle event
                popup.Closed += PopupClosed;

                // disable popup background clickthrough, contribution : WORDIBOI
                popup.MouseDown += onMousedown;

                // disable datagrid while popup is open
                IsEnabled = false;

                // resizable grid
                sizableContentGrid = VisualTreeHelpers.FindChild<Grid>(popup.Child, "SizableContentGrid");

                // search textbox
                searchTextBox = VisualTreeHelpers.FindChild<TextBox>(popup.Child, "SearchBox");
                searchTextBox.Text = string.Empty;
                searchTextBox.TextChanged += SearchTextBoxOnTextChanged;
                searchTextBox.Focusable = true;

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
                }

                if (columnType == typeof(DataGridTemplateColumn))
                {
                    var column = (DataGridTemplateColumn)header.Column;
                    fieldName = column.FieldName;
                }

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                // get type of field
                fieldType = null;
                var fieldProperty = collectionType.GetProperty(fieldName);

                // get type or underlying type if nullable
                if (fieldProperty != null)
                    fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;

                if (fieldType == null) return;

                // if current is not in filterManager, create
                filterManager.SetCurrent(fieldName, fieldType);

                if (filterManager.CurrentFilter == null) return;

                // disable sorting
                Sorted -= OnSorted;

                var filterItemList = new List<FilterItem>();

                Mouse.OverrideCursor = Cursors.Wait;

                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        //filterItemList = GetColumnValuesWhileLoop(fieldProperty);
                        filterItemList = GetColumnValuesForLoop(fieldProperty);
                    });
                });

                var commonItemList = new List<FilterItem>
                {
                    new FilterItem { Content = "(Select All)", Initialize = true, Level = 0 }
                };

                // TreeView
                if (fieldType == typeof(DateTime))
                {
                    commonItemList.AddRange(filterItemList);

                    // fill the treeview with BuildTreeView
                    TreeviewItems = BuildTreeView(filterItemList);
                }
                else
                {
                    // ListBox
                    commonItemList.AddRange(filterItemList.Where(c => c.Content != null));

                    // add empty item
                    var empty = filterItemList.FirstOrDefault(c => c.Content == null);
                    if (empty != null)
                        commonItemList.Add(new FilterItem
                        {
                            Content = Translate.Empty,
                            FieldType = fieldType,
                            GroupIndex = empty.GroupIndex,
                            Initialize = empty.IsChecked,
                            IsPrevious = empty.IsPrevious,
                            Level = -1
                        });

                    ListBoxItems = commonItemList;
                }

                // Set ICollectionView for filtering in the pop-up window
                ItemCollectionView = CollectionViewSource.GetDefaultView(commonItemList);

                // set filter in popup
                if (ItemCollectionView.CanFilter) ItemCollectionView.Filter = SearchFilter;

                // set the placement and offset of the PopUp in relation to the header and the main window of the application
                // i.e (placement : bottom left or bottom right)
                PopupPlacement(sizableContentGrid, header);

                popup.UpdateLayout();

                // open popup
                popup.IsOpen = true;

                // set focus on searchTextBox
                searchTextBox.Focus();
                Keyboard.Focus(searchTextBox);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ShowFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // reset cursor
                ResetCursor();

                stopWatchFilter.Stop();

                // show open popup elapsed time in UI
                ElapsedTime = stopWatchFilter.Elapsed;
            }
        }

        #endregion Private Methods
    }
}