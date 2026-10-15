#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : FilterDataGrid.Net
// File       : FilterDataGrid.cs
// Created    : 06/03/2022
//

// Refactored by Dlico23

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

// ReSharper disable All
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UseNameofForDependencyProperty
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment
// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace FilterDataGrid
{
    /// <summary>
    ///     Implementation of Datagrid
    /// </summary>
    public class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        ///     FilterDataGrid constructor
        /// </summary>
        public FilterDataGrid()
        {
            Debug.WriteLineIf(DebugMode, "FilterDataGrid.Constructor");

            DefaultStyleKey = typeof(FilterDataGrid);

            // load resources
            ResourceDictionary resourceDictionary = new()
            {
                Source = new Uri(ResourceDictionaryPath, UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(resourceDictionary);

            // initial popup size
            popUpSize = new Point
            {
                X = (double)TryFindResource(PopupWidthKey),
                Y = (double)TryFindResource(PopupHeightKey)
            };

            CommandBindings.Add(new CommandBinding(ApplyFilter, ApplyFilterCommand, CanApplyFilter)); // Ok
            CommandBindings.Add(new CommandBinding(CancelFilter, CancelFilterCommand));
            CommandBindings.Add(new CommandBinding(ClearSearchBox, ClearSearchBoxClick));
            CommandBindings.Add(new CommandBinding(IsChecked, CheckedAllCommand));
            CommandBindings.Add(new CommandBinding(RemoveAllFilters, RemoveAllFilterCommand, CanRemoveAllFilter));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterCommand, CanRemoveFilter));
            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand, CanShowFilter));

            Loaded += (s, e) => OnLoadFilterDataGrid(this, new DependencyPropertyChangedEventArgs());
        }

        static FilterDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterDataGrid), new
                FrameworkPropertyMetadata(typeof(FilterDataGrid)));
        }

        #endregion Constructors

        #region Command

        public static readonly ICommand ApplyFilter = new RoutedCommand();
        public static readonly ICommand CancelFilter = new RoutedCommand();
        public static readonly ICommand ClearSearchBox = new RoutedCommand();
        public static readonly ICommand IsChecked = new RoutedCommand();
        public static readonly ICommand RemoveAllFilters = new RoutedCommand();
        public static readonly ICommand RemoveFilter = new RoutedCommand();
        public static readonly ICommand ShowFilter = new RoutedCommand();

        #endregion Command

        #region Public DependencyProperty

        /// <summary>
        ///     Excluded Fields (only AutoGeneratingColumn)
        /// </summary>
        public static readonly DependencyProperty ExcludeFieldsProperty =
            DependencyProperty.Register("ExcludeFields",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(string.Empty));

        /// <summary>
        ///     Excluded Column (only AutoGeneratingColumn)
        /// </summary>
        public static readonly DependencyProperty ExcludeColumnsProperty =
            DependencyProperty.Register("ExcludeColumns",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(string.Empty));

        /// <summary>
        ///     Date format displayed
        /// </summary>
        public static readonly DependencyProperty DateFormatStringProperty =
            DependencyProperty.Register("DateFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata("d"));

        /// <summary>
        ///     Language displayed
        /// </summary>
        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local),
                typeof(FilterDataGrid),
                new PropertyMetadata(Local.English));

        /// <summary>
        ///     Show elapsed time in status bar
        /// </summary>
        public static readonly DependencyProperty ShowElapsedTimeProperty =
            DependencyProperty.Register("ShowElapsedTime",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Show status bar
        /// </summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register("ShowStatusBar",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Show Rows Count
        /// </summary>
        public static readonly DependencyProperty ShowRowsCountProperty =
            DependencyProperty.Register("ShowRowsCount",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Persistent filter
        /// </summary>
        public static readonly DependencyProperty PersistentFilterProperty =
            DependencyProperty.Register("PersistentFilter",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Filter popup background property.
        ///     Allows the user to set a custom background color for the filter popup. When nothing is set, the default value is background color of host windows.
        /// </summary>
        public static readonly DependencyProperty FilterPopupBackgroundProperty =
            DependencyProperty.Register("FilterPopupBackground",
                typeof(Brush),
                typeof(FilterDataGrid),
                new PropertyMetadata(null));

        #endregion Public DependencyProperty

        #region Public Event

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Sorted;

        /// <summary>
        /// Raised before ItemsSource is about to change
        /// Allows cancellation of state preservation
        /// </summary>
        public event EventHandler<ItemsSourceChangingEventArgs> ItemsSourceChanging;

        /// <summary>
        /// Raised after ItemsSource has changed and state has been restored (if enabled)
        /// </summary>
        public event EventHandler<ItemsSourceChangedEventArgs> ItemsSourceChanged;

        /// <summary>
        /// Raised before filtered items are about to change (before filter is applied/removed)
        /// Provides current and upcoming filtered item counts
        /// </summary>
        public event EventHandler<FilteredItemsChangingEventArgs> FilteredItemsChanging;

        /// <summary>
        /// Raised after filtered items have changed (after filter is applied/removed)
        /// Provides previous and current filtered item counts
        /// </summary>
        public event EventHandler<FilteredItemsChangedEventArgs> FilteredItemsChanged;

        /// <summary>
        /// Unified event raised whenever any data changes (ItemsSource OR FilteredItems)
        /// Perfect for monitoring all data changes in one place
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;

        #endregion Public Event

        #region Private Constants

        private const bool DebugMode = true;
        private const string ResourceDictionaryPath = "/FilterDataGrid;component/Themes/Generic.xaml";
        private const string PopupWidthKey = "PopupWidth";
        private const string PopupHeightKey = "PopupHeight";
        private const string DataGridHeaderTemplateKey = "DataGridHeaderTemplate";
        private const string FilterButtonKey = "FilterButton";
        private const string FilterPopupKey = "FilterPopup";
        private const string SizableContentGridKey = "SizableContentGrid";
        private const string SearchBoxKey = "SearchBox";
        private const string PopupThumbKey = "PopupThumb";
        private const string ScrollViewerKey = "DG_ScrollViewer";
        private const string JsonFileExtension = ".json";
        private const string PersistentFilterFileName = "persistentFilter.json";
        private const double DefaultRowHeaderWidth = 6.0;
        private new const double BorderThickness = 1.0;
        private const double RowHeaderMargin = 2.0;
        private const double RowHeaderPadding = 4.0;
        private const int TypeCodeMinNumeric = 5;
        private const int TypeCodeMaxNumeric = 15;
        private const int SelectAllLevel = 0;
        private const int EmptyItemLevel = -1;
        private const int StandardItemLevel = 1;

        #endregion Private Constants

        #region Private Fields

        private string fileName = PersistentFilterFileName;
        private Stopwatch stopWatchFilter = new();
        private DataGridColumnHeadersPresenter columnHeadersPresenter;
        private bool currentlyFiltering;
        private bool search;
        private Button button;

        private Cursor cursor;
        private int searchLength;
        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;
        private Grid sizableContentGrid;

        private List<string> excludedFields;
        private List<string> excludedColumns;
        private List<FilterItemDate> treeView;
        private List<FilterItem> listBoxItems;

        private Point popUpSize;
        private Popup popup;

        private string fieldName;
        private string lastFilter;
        private string searchText;
        private TextBox searchTextBox;
        private Thumb thumb;

        private TimeSpan elapsed;
        private Type fieldType;

        private bool startsWith;

        private readonly Dictionary<string, Predicate<object>> criteria = [];

        private bool isPreservingState;
        private object selectedItemBeforeChange;
        private int selectedIndexBeforeChange;
        private double verticalOffsetBeforeChange;
        private double horizontalOffsetBeforeChange;
        private List<FilterCommon> filterStateBeforeChange;

        #endregion Private Fields

        #region Public Properties

        public Type CollectionType { get; private set; } //Make this public with a private setter so it can be accessed by classes that inherit from filter datagrid.

        /// <summary>
        ///     Excluded Fields (AutoGeneratingColumn)
        /// </summary>
        public string ExcludeFields
        {
            get => (string)GetValue(ExcludeFieldsProperty);
            set => SetValue(ExcludeFieldsProperty, value);
        }

        /// <summary>
        ///     Excluded Columns (AutoGeneratingColumn)
        /// </summary>
        public string ExcludeColumns
        {
            get => (string)GetValue(ExcludeColumnsProperty);
            set => SetValue(ExcludeColumnsProperty, value);
        }

        /// <summary>
        ///     The string begins with the specific character. Used in pop-up search box
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
            get => elapsed;
            set
            {
                elapsed = value;
                OnPropertyChanged();
            }
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

        /// <summary>
        ///     Show elapsed time in status bar
        /// </summary>
        public bool ShowElapsedTime
        {
            get => (bool)GetValue(ShowElapsedTimeProperty);
            set => SetValue(ShowElapsedTimeProperty, value);
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
        ///     Show rows count
        /// </summary>
        public bool ShowRowsCount
        {
            get => (bool)GetValue(ShowRowsCountProperty);
            set => SetValue(ShowRowsCountProperty, value);
        }

        /// <summary>
        ///     Instance of Loc
        /// </summary>
        public Loc Translate { get; set; }

        /// <summary>
        /// Tree View ItemsSource
        /// </summary>
        public List<FilterItemDate> TreeViewItems
        {
            get => treeView ?? [];
            set
            {
                treeView = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ListBox ItemsSource
        /// </summary>
        public List<FilterItem> ListBoxItems
        {
            get => listBoxItems ?? [];
            set
            {
                listBoxItems = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Field Type
        /// </summary>
        public Type FieldType
        {
            get => fieldType;
            set
            {
                fieldType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Persistent filter
        /// </summary>
        public bool PersistentFilter
        {
            get => (bool)GetValue(PersistentFilterProperty);
            set => SetValue(PersistentFilterProperty, value);
        }

        /// <summary>
        ///     Filter pop-up background
        /// </summary>
        public Brush FilterPopupBackground
        {
            get => (Brush)GetValue(FilterPopupBackgroundProperty);
            set => SetValue(FilterPopupBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the current active filters as a read-only collection
        /// </summary>
        public IReadOnlyList<FilterCommon> ActiveFilters => GlobalFilterList.AsReadOnly();

        /// <summary>
        /// Gets the current filtered items from the CollectionView
        /// Returns the filtered view of the ItemsSource
        /// </summary>
        public IEnumerable FilteredItems => CollectionViewSource ?? Items;

        /// <summary>
        /// Gets the count of filtered items
        /// </summary>
        public int FilteredItemsCount
        {
            get
            {
                if (CollectionViewSource == null) return Items.Count;

                int count = 0;
                foreach (object item in CollectionViewSource)
                {
                    count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets or sets whether to preserve filter and view state when ItemsSource changes
        /// Default is true
        /// </summary>
        public bool PreserveStateOnItemsSourceChange { get; set; } = true;

        #endregion Public Properties

        #region Private Properties

        private FilterCommon CurrentFilter { get; set; }
        private ICollectionView CollectionViewSource { get; set; }
        private ICollectionView ItemCollectionView { get; set; }
        private List<FilterCommon> GlobalFilterList { get; } = [];

        /// <summary>
        /// Popup filtered items (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> PopupViewItems =>
            ItemCollectionView?.OfType<FilterItem>().Where(c => c.Level != SelectAllLevel) ?? Enumerable.Empty<FilterItem>();

        /// <summary>
        /// Popup source collection (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> SourcePopupViewItems =>
            ItemCollectionView?.SourceCollection.OfType<FilterItem>().Where(c => c.Level != SelectAllLevel) ??
            Enumerable.Empty<FilterItem>();

        #endregion Private Properties

        #region Protected Methods

        // CALL ORDER :
        // Constructor
        // OnInitialized
        // OnItemsSourceChanged
        // OnLoaded

        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            Debug.WriteLineIf(DebugMode, $"OnInitialized :{Name}");

            base.OnInitialized(e);

            try
            {
                // FilterLanguage : default : 0 (english)
                Translate = new Loc { Language = FilterLanguage };

                // fill excluded Fields list with values
                if (AutoGenerateColumns)
                {
                    excludedFields = [.. ExcludeFields.Split(',').Select(p => p.Trim())];
                    excludedColumns = [.. ExcludeColumns.Split(',').Select(p => p.Trim())];
                }
                // generating custom columns
                else if (CollectionType != null) GeneratingCustomsColumn();

                // sorting event
                Sorted += OnSorted;
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
            Debug.WriteLineIf(DebugMode, $"OnAutoGeneratingColumn : {e.PropertyName}");

            base.OnAutoGeneratingColumn(e);

            try
            {
                // ignore excluded columns
                if (excludedColumns.Any(x => string.Equals(x, e.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    e.Cancel = true;
                    return;
                }

                // enable column sorting when user specified
                e.Column.CanUserSort = CanUserSortColumns;

                // return if the field is excluded
                if (excludedFields.Any(c => string.Equals(c, e.PropertyName, StringComparison.CurrentCultureIgnoreCase))) return;

                // template
                DataTemplate template = (DataTemplate)TryFindResource(DataGridHeaderTemplateKey);

                // get type
                fieldType = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                // get type code
                TypeCode typeCode = Type.GetTypeCode(fieldType);

                if (fieldType.IsEnum)
                {
                    DataGridComboBoxColumn column = new()
                    {
                        ItemsSource = ((System.Windows.Controls.DataGridComboBoxColumn)e.Column).ItemsSource,
                        SelectedItemBinding = new Binding(e.PropertyName),
                        FieldName = e.PropertyName,
                        Header = e.Column.Header,
                        HeaderTemplate = template,
                        IsSingle = false, // eNum is not a unique value (unique identifier)
                        IsColumnFiltered = true
                    };

                    e.Column = column;
                }
                else if (typeCode == TypeCode.Boolean)
                {
                    DataGridCheckBoxColumn column = new()
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header,
                        HeaderTemplate = template,
                        IsColumnFiltered = true
                    };

                    e.Column = column;
                }
                // TypeCode of numeric type, between 5 and 15
                else if ((int)typeCode > TypeCodeMinNumeric - 1 && (int)typeCode < TypeCodeMaxNumeric + 1)
                {
                    DataGridNumericColumn column = new()
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header,
                        HeaderTemplate = template,
                        IsColumnFiltered = true
                    };

                    e.Column = column;
                }
                else
                {
                    DataGridTextColumn column = new()
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header,
                        IsColumnFiltered = true
                    };

                    // apply the format string provided
                    if (typeCode == TypeCode.DateTime && !string.IsNullOrEmpty(DateFormatString))
                        column.Binding.StringFormat = DateFormatString;

                    // if the type does not belong to the "System" namespace, disable sorting
                    if (!fieldType.IsSystemType())
                    {
                        column.CanUserSort = false;

                        // if the type is a nested object (class), disable cell editing
                        column.IsReadOnly = fieldType.IsClass;
                    }
                    else
                    {
                        column.HeaderTemplate = template;
                    }

                    e.Column = column;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnAutoGeneratingColumn : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     The source of the Data grid items has been changed (refresh or on loading)
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Debug.WriteLineIf(DebugMode, $"\nOnItemsSourceChanged Auto : {AutoGenerateColumns}");

            // Handle state preservation
            if (PreserveStateOnItemsSourceChange && oldValue != null && newValue != null)
            {
                ItemsSourceChangingEventArgs changingArgs = new(oldValue, newValue);
                OnItemsSourceChanging(changingArgs);

                if (!changingArgs.Cancel)
                {
                    CaptureCurrentState();
                    isPreservingState = true;
                }
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            try
            {
                // remove previous event : Contribution mcboothy
                if (oldValue is INotifyCollectionChanged collectionChanged)
                    collectionChanged.CollectionChanged -= ItemSourceCollectionChanged;

                if (newValue == null)
                {
                    RemoveFilters();

                    // remove custom HeaderTemplate
                    foreach (DataGridColumn col in Columns)
                    {
                        col.HeaderTemplate = null;
                    }
                    return;
                }

                if (oldValue != null)
                {
                    RemoveFilters();

                    // free previous resource
                    CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());

                    // scroll to top on reload collection
                    ScrollViewer scrollViewer = GetTemplateChild(ScrollViewerKey) as ScrollViewer;
                    scrollViewer?.ScrollToTop();
                }

                // add new event : Contribution mcboothy
                if (newValue is INotifyCollectionChanged changed)
                    changed.CollectionChanged += ItemSourceCollectionChanged;

                CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);

                // set Filter, contribution : STEFAN HEIMEL
                if (CollectionViewSource.CanFilter) CollectionViewSource.Filter = Filter;

                ItemsSourceCount = Items.Count;
                ElapsedTime = new TimeSpan(0, 0, 0);

                OnPropertyChanged(nameof(ItemsSourceCount));
                OnPropertyChanged(nameof(GlobalFilterList));

                // Calculate row header width
                if (ShowRowsCount)
                {
                    TextBlock txt = new()
                    {
                        Text = ItemsSourceCount.ToString(),
                        FontSize = FontSize,
                        FontFamily = FontFamily,
                        Padding = new Thickness(0, 0, RowHeaderPadding, 0),
                        Margin = new Thickness(RowHeaderMargin)
                    };
                    txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    RowHeaderWidth = Math.Max(Math.Ceiling(txt.DesiredSize.Width),
                        RowHeaderWidth >= 0 ? RowHeaderWidth : 0);
                }
                else
                {
                    // default value, if this value is set to 0, the row header is not displayed
                    // and the exception occurs when the value is set to < 0
                    RowHeaderWidth = DefaultRowHeaderWidth;
                }

                // get collection type
                // contribution : APFLKUACHA
                CollectionType = ItemsSource is ICollectionView collectionView
                    ? collectionView.SourceCollection?.GetType().GenericTypeArguments.FirstOrDefault()
                    : ItemsSource?.GetType().GenericTypeArguments.FirstOrDefault();

                // set name of persistent filter json file
                // the name of the file is defined by the "Name" property of the FilterDatGrid, otherwise
                // the name of the source collection type is used
                if (CollectionType != null)
                    fileName = !string.IsNullOrEmpty(Name) ? $"{Name}{JsonFileExtension}" : $"{CollectionType?.Name}{JsonFileExtension}";

                // generating custom columns
                // Will allow both autogenerated columns and columns added through xaml.
                // Includes checking for column.IsAutoGenerated in GeneratingCustomsColumn()
                if (CollectionType != null) GeneratingCustomsColumn();

                // re-evalutate the command's CanExecute.
                // when "IsReadOnly" is set to "False", "CanRemoveAllFilter" is not re-evaluated,
                // the "Remove All Filters" icon remains active
                CommandManager.InvalidateRequerySuggested();

                // Restore state if preservation was active
                if (isPreservingState && newValue != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RestoreState();
                        isPreservingState = false;

                        OnItemsSourceChanged(new ItemsSourceChangedEventArgs(oldValue, newValue, true));
                    }), DispatcherPriority.Loaded);
                }
                else if (newValue != null)
                {
                    OnItemsSourceChanged(new ItemsSourceChangedEventArgs(oldValue, newValue, false));
                }
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
            if (currentlyFiltering || (popup?.IsOpen ?? false)) return;

            Mouse.OverrideCursor = Cursors.Wait;
            base.OnSorting(eventArgs);
            Sorted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Adding Rows count
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            if (ShowRowsCount)
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Access by the Host application to the method of loading active filters
        /// </summary>
        public void LoadPreset()
        {
            DeSerialize();
        }

        /// <summary>
        /// Access by the Host application to the method of saving active filters
        /// </summary>
        public void SavePreset()
        {
            Serialize();
        }

        /// <summary>
        ///     Remove All Filters
        /// </summary>
        public void RemoveFilters()
        {
            Debug.WriteLineIf(DebugMode, "RemoveFilters");

            ElapsedTime = new TimeSpan(0, 0, 0);

            // Capture current count before removing filters
            int previousFilteredCount = FilteredItemsCount;

            try
            {
                // Raise FilteredItemsChanging before removing filters
                OnFilteredItemsChanging(new FilteredItemsChangingEventArgs(previousFilteredCount, Items.Count)); // Will show all items

                foreach (FilterCommon filter in GlobalFilterList)
                {
                    FilterState.SetIsFiltered(filter.FilterButton, false);
                }

                // reset current filter
                CurrentFilter = null;
                criteria.Clear();
                GlobalFilterList.Clear();
                CollectionViewSource?.Refresh();

                // Raise FilteredItemsChanged after removing filters
                int currentFilteredCount = FilteredItemsCount;
                OnFilteredItemsChanged(new FilteredItemsChangedEventArgs(previousFilteredCount, currentFilteredCount));

                // empty json file
                if (PersistentFilter) SavePreset();
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugMode, $"RemoveFilters error : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the current filter state as a serializable object
        /// Can be used to save/restore filter state programmatically
        /// </summary>
        /// <returns>Filter state data that can be serialized</returns>
        public FilterStateData GetCurrentFilterState()
        {
            FilterStateData stateData = new()
            {
                Filters = []
            };

            IReadOnlyList<FilterCommon> activeFilters = ActiveFilters;

            foreach (FilterCommon filter in activeFilters)
            {
                FilterData filterData = new()
                {
                    FieldName = filter.FieldName,
                    FieldTypeName = filter.FieldType?.AssemblyQualifiedName,
                    FilteredItems = [.. filter.PreviouslyFilteredItems]
                };

                stateData.Filters.Add(filterData);
            }

            return stateData;
        }

        /// <summary>
        /// Applies filters programmatically from filter state data
        /// </summary>
        /// <param name="filterState">The filter state to apply</param>
        public void ApplyFilterState(FilterStateData filterState)
        {
            if (filterState == null || filterState.Filters == null || filterState.Filters.Count == 0)
            {
                RemoveFilters();
                return;
            }

            // Clear existing filters first
            RemoveFilters();

            List<FilterCommon> filtersToApply = [];

            foreach (FilterData filterData in filterState.Filters)
            {
                Type fieldType = null;
                if (!string.IsNullOrEmpty(filterData.FieldTypeName))
                {
                    fieldType = Type.GetType(filterData.FieldTypeName);
                }

                if (fieldType == null) continue;

                FilterCommon filter = new()
                {
                    FieldName = filterData.FieldName,
                    FieldType = fieldType,
                    Translate = Translate,
                    PreviouslyFilteredItems = [.. filterData.FilteredItems]
                };

                filtersToApply.Add(filter);
            }

            // Apply filters through the internal mechanism
            ApplyFiltersInternal(filtersToApply);
        }

        /// <summary>
        /// Creates a filter builder for programmatic filter construction
        /// </summary>
        /// <returns>A new filter builder instance</returns>
        public FilterBuilder CreateFilterBuilder()
        {
            return new FilterBuilder(this);
        }

        /// <summary>
        /// Refreshes the current filter without changing filter state
        /// Useful when ItemsSource data has been modified
        /// </summary>
        public void RefreshFilter()
        {
            // Capture current count before refresh
            int previousFilteredCount = FilteredItemsCount;

            // Raise FilteredItemsChanging before refresh
            OnFilteredItemsChanging(new FilteredItemsChangingEventArgs(previousFilteredCount, -1)); // -1 = unknown upcoming count

            CollectionViewSource?.Refresh();

            // Raise FilteredItemsChanged after refresh
            int currentFilteredCount = FilteredItemsCount;
            OnFilteredItemsChanged(new FilteredItemsChangedEventArgs(previousFilteredCount, currentFilteredCount));
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        ///    Event handler for the "Loaded" event of the "FrameworkContentElement" class.
        /// </summary>
        /// <param name="filterDataGrid"></param>
        /// <param name="e"></param>
        private void OnLoadFilterDataGrid(FilterDataGrid filterDataGrid, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, $"\tOnLoadFilterDataGrid {filterDataGrid?.Name}");

            base.OnApplyTemplate();

            if (filterDataGrid == null) return;

            Window hostingWindow = Window.GetWindow(this);

            // set the background color of the filter popup
            FilterPopupBackground = FilterPopupBackground == null && hostingWindow != null
                ? hostingWindow.Background
                : new SolidColorBrush(Colors.White);

            if (filterDataGrid.PersistentFilter)
                filterDataGrid.LoadPreset();
        }

        /// <summary>
        ///     Restore filters from json file
        ///     contribution : ericvdberge
        /// </summary>
        /// <param name="filterPreset">all the saved filters from a FilterDataGrid</param>
        private void OnFilterPresetChanged(List<FilterCommon> filterPreset)
        {
            Debug.WriteLineIf(DebugMode, "OnFilterPresetChanged");

            if (filterPreset == null || filterPreset.Count == 0) return;

            // Set cursor to wait
            Mouse.OverrideCursor = Cursors.Wait;

            // Remove all existing filters
            if (GlobalFilterList.Count > 0)
                RemoveFilters();

            // Reset previous elapsed time
            ElapsedTime = new TimeSpan(0, 0, 0);
            stopWatchFilter = Stopwatch.StartNew();

            try
            {
                foreach (FilterCommon preset in filterPreset)
                {
                    // Get columns that match the preset field name and are filterable
                    List<DataGridColumn> columns = [.. Columns
                        .Where(c =>
                            (c is DataGridTextColumn dtx && dtx.IsColumnFiltered && dtx.FieldName == preset.FieldName)
                            || (c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered && dtp.FieldName == preset.FieldName)
                            || (c is DataGridCheckBoxColumn dck && dck.IsColumnFiltered && dck.FieldName == preset.FieldName)
                            || (c is DataGridNumericColumn dnm && dnm.IsColumnFiltered && dnm.FieldName == preset.FieldName)
                            || (c is DataGridComboBoxColumn cmb && cmb.IsColumnFiltered && cmb.FieldName == preset.FieldName))];

                    foreach (DataGridColumn col in columns)
                    {
                        // Get distinct values from the ItemsSource for the current column
                        List<object> sourceObjectList = preset.FieldType == typeof(DateTime)
                            ? [.. Items.Cast<object>()
                                .Select(x => (object)((DateTime?)x.GetPropertyValue(preset.FieldName))?.Date)
                                .Distinct()]
                            : [.. Items.Cast<object>()
                                .Select(x => x.GetPropertyValue(preset.FieldName))
                                .Distinct()];

                        // Convert previously filtered items to the correct type
                        preset.PreviouslyFilteredItems = [.. preset.PreviouslyFilteredItems.Select(o => FilterDataGrid.ConvertToType(o, preset.FieldType))];

                        // Get the items that are always present in the source collection
                        preset.FilteredItems = [.. sourceObjectList.Where(c => preset.PreviouslyFilteredItems.Contains(c))];

                        // if no items are filtered, continue to the next column
                        if (preset.FilteredItems.Count == 0)
                            continue;

                        preset.Translate = Translate;

                        Button filterButton = VisualTreeHelpers.GetHeader(col, this)
                            ?.FindVisualChild<Button>(FilterButtonKey);

                        preset.FilterButton = filterButton;

                        FilterState.SetIsFiltered(filterButton, true);

                        preset.AddFilter(criteria);

                        // Add current filter to GlobalFilterList
                        if (GlobalFilterList.All(f => f.FieldName != preset.FieldName))
                            GlobalFilterList.Add(preset);

                        // Set the current field name as the last filter name
                        lastFilter = preset.FieldName;
                    }
                }

                // Remove all predefined filters when there is no match with the source collection
                if (filterPreset.Count == 0)
                    RemoveFilters();

                // Save json file
                SavePreset();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnFilterPresetChanged : {ex.Message}");
                throw;
            }
            finally
            {
                // Apply filter
                CollectionViewSource.Refresh();

                stopWatchFilter.Stop();

                // Show elapsed time in UI
                ElapsedTime = stopWatchFilter.Elapsed;

                // Reset cursor
                ResetCursor();

                Debug.WriteLineIf(DebugMode, $"OnFilterPresetChanged Elapsed time : {ElapsedTime:mm\\:ss\\.ff}");
            }
        }

        /// <summary>
        /// Convert an object to the specified type.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The converted object.</returns>
        private static object ConvertToType(object value, Type type)
        {
            try
            {
                if (type == typeof(DateTime))
                {
                    return DateTime.TryParse(value?.ToString(), out DateTime dateTime) ? (object)dateTime : (object)(DateTime?)null;
                }
                if (type.IsEnum)
                {
                    return Enum.Parse(type, value.ToString());
                }
                return Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConvertToType error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Serialize filters list
        /// </summary>
        private async void Serialize()
        {
            await Task.Run(() =>
            {
                long result = JsonConvert.Serialize(fileName, GlobalFilterList);
                Debug.WriteLineIf(DebugMode, $"Serialize : {result}");
            });
        }

        /// <summary>
        /// Deserialize json file
        /// </summary>
        private async void DeSerialize()
        {
            await Task.Run(() =>
            {
                List<FilterCommon> result = JsonConvert.Deserialize<List<FilterCommon>>(fileName);

                if (result == null) return;
                Dispatcher.BeginInvoke((Action)(() => { OnFilterPresetChanged(result); }),
                    DispatcherPriority.Normal);

                Debug.WriteLineIf(DebugMode, $"DeSerialize : {result.Count}");
            });
        }

        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        private async Task<List<FilterItemDate>> BuildTreeAsync(IEnumerable<FilterItem> dates)
        {
            List<FilterItemDate> tree =
            [
                new FilterItemDate
                {
                    Label = Translate.All, Level = SelectAllLevel, Initialize = true, FieldType = fieldType
                }
            ];

            if (dates == null) return tree;

            try
            {
                List<FilterItem> dateTimes = [.. dates.Where(x => x.Level > SelectAllLevel)];

                List<FilterItemDate> years = [.. dateTimes.GroupBy(
                    x => ((DateTime)x.Content).Year,
                    (key, group) => new FilterItemDate
                    {
                        Level = StandardItemLevel,
                        Content = key,
                        Label = key.ToString(Translate.Culture),
                        Initialize = true,
                        FieldType = fieldType,
                        Children = [.. group.GroupBy(
                            x => ((DateTime)x.Content).Month,
                            (monthKey, monthGroup) => new FilterItemDate
                            {
                                Level = 2,
                                Content = monthKey,
                                Label = new DateTime(key, monthKey, 1).ToString("MMMM", Translate.Culture),
                                Initialize = true,
                                FieldType = fieldType,
                                Children = [.. monthGroup.Select(x => new FilterItemDate
                                {
                                    Level = 3,
                                    Content = ((DateTime)x.Content).Day,
                                    Label = ((DateTime)x.Content).ToString("dd", Translate.Culture),
                                    Initialize = true,
                                    FieldType = fieldType,
                                    Item = x
                                })]
                            })]
                    })];

                foreach (FilterItemDate year in years)
                {
                    foreach (FilterItemDate month in year.Children)
                    {
                        month.Parent = year;
                        foreach (FilterItemDate day in month.Children)
                        {
                            day.Parent = month;
                            // set the state of the "IsChecked" property based on the items already filtered (unchecked)
                            if (!day.Item.IsChecked)
                            {
                                // call the SetIsChecked method of the FilterItemDate class
                                day.IsChecked = false;
                                // reset with new state (isChanged == false)
                                day.Initialize = day.IsChecked;
                            }
                        }
                        // reset with new state
                        month.Initialize = month.IsChecked;
                    }
                    // reset with new state
                    year.Initialize = year.IsChecked;
                }

                tree.AddRange(years);

                if (dates.Any(x => x.Level == EmptyItemLevel))
                {
                    FilterItem emptyItem = dates.First(x => x.Level == EmptyItemLevel);
                    tree.Add(new FilterItemDate
                    {
                        Label = Translate.Empty,
                        Content = null,
                        Level = EmptyItemLevel,
                        FieldType = fieldType,
                        Initialize = emptyItem.IsChecked,
                        Item = emptyItem,
                        Children = []
                    });
                }

                tree.First().Tree = tree;
                return await Task.FromResult(tree);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Handle Mousedown, contribution : WORDIBOI
        /// </summary>
        private readonly MouseButtonEventHandler onMousedown = (_, eArgs) => { eArgs.Handled = true; };

        /// <summary>
        ///     Generate custom columns that can be filtered
        /// </summary>
        private void GeneratingCustomsColumn()
        {
            Debug.WriteLineIf(DebugMode, "GeneratingCustomColumn");

            try
            {
                PropertyInfo fieldProperty = null;

                // ReSharper disable MergeIntoPattern

                // NOTE: Combined linq and foreach to prevent iterating over the columns multiple times.
                // set header template
                foreach (DataGridColumn dataGridColumn in Columns)
                {
                    // Skipping autogenered columns. See note in OnItemsSourceChanged()
                    if (dataGridColumn.IsAutoGenerated)
                        continue;

                    // Cast as interface to check commmon column properties.

                    if (dataGridColumn is not IDataGridColumn col || !col.IsColumnFiltered)
                        continue;

                    if (dataGridColumn.HeaderTemplate != null)
                    {
                        // Debug.WriteLineIf(DebugMode, "\tReset filter Button");

                        // reset filter Button
                        Button buttonFilter = VisualTreeHelpers.GetHeader(dataGridColumn, this)
                            ?.FindVisualChild<Button>(FilterButtonKey);

                        if (buttonFilter != null) FilterState.SetIsFiltered(buttonFilter, false);

                        // update the "ComboBoxItemsSource" custom property of "DataGridComboBoxColumn"
                        // this collection may change when loading a new source collection of the DataGrid.
                        if (col is DataGridComboBoxColumn comboBoxColumn) // Change to cast so that inherting column types can be used.
                        {
                            if (comboBoxColumn.IsSingle)
                            {
                                comboBoxColumn.UpdateItemsSourceAsync();
                            }
                        }
                    }
                    else
                    {
                        // Debug.WriteLineIf(DebugMode, "\tGenerate Columns");

                        fieldType = null;
                        DataTemplate template = (DataTemplate)TryFindResource(DataGridHeaderTemplateKey);

                        // Convert to switch, only one case should ever get used, so checking for each type should be uneccessary.
                        // Change to cast so inherited column typs can be used.
                        switch (col)
                        {
                            case DataGridTemplateColumn templateColumn:

                                // DataGridTemplateColumn has no culture property
                                if (string.IsNullOrEmpty(templateColumn.FieldName))
                                    throw new ArgumentException("Value of \"FieldName\" property cannot be null.",
                                        nameof(DataGridTemplateColumn));

                                // template
                                templateColumn.HeaderTemplate = template;
                                break;

                            // Should check for both is textColumn and is NumericColumn to insure the correct case is used.
                            case DataGridTextColumn textColumn when col is DataGridNumericColumn numericColumn:
                                numericColumn.FieldName = ((Binding)numericColumn.Binding).Path.Path;

                                // template
                                numericColumn.HeaderTemplate = template;

                                // culture
                                if (((Binding)numericColumn.Binding).ConverterCulture == null)
                                    ((Binding)numericColumn.Binding).ConverterCulture = Translate.Culture;
                                break;

                            case DataGridTextColumn textColumn:

                                textColumn.FieldName = ((Binding)textColumn.Binding).Path.Path;

                                // template
                                textColumn.HeaderTemplate = template;

                                fieldProperty =
                                    CollectionType.GetProperty(((Binding)textColumn.Binding).Path.Path);

                                // get type or underlying type if nullable
                                if (fieldProperty != null)
                                    fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                                fieldProperty.PropertyType;

                                // apply DateFormatString when StringFormat for column is not provided or empty
                                if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                                    if (string.IsNullOrEmpty(textColumn.Binding.StringFormat))
                                        textColumn.Binding.StringFormat = DateFormatString;

                                FieldType = fieldType;

                                // culture
                                if (((Binding)textColumn.Binding).ConverterCulture == null)
                                    ((Binding)textColumn.Binding).ConverterCulture = Translate.Culture;
                                break;

                            case DataGridCheckBoxColumn checkBoxColumn:
                                checkBoxColumn.FieldName = ((Binding)checkBoxColumn.Binding).Path.Path;

                                // template
                                checkBoxColumn.HeaderTemplate = template;

                                // culture
                                if (((Binding)checkBoxColumn.Binding).ConverterCulture == null)
                                    ((Binding)checkBoxColumn.Binding).ConverterCulture = Translate.Culture;
                                break;

                            case DataGridComboBoxColumn comboBoxColumn:

                                if (comboBoxColumn.ItemsSource == null)
                                    continue;

                                Binding binding = (Binding)comboBoxColumn.SelectedValueBinding ??
                                              (Binding)comboBoxColumn.SelectedItemBinding;

                                // check if binding is missing
                                if (binding != null)
                                {
                                    comboBoxColumn.FieldName = binding.Path.Path;

                                    // template
                                    comboBoxColumn.HeaderTemplate = template;

                                    fieldProperty = CollectionType.GetPropertyInfo(comboBoxColumn.FieldName);

                                    // get type or underlying type if nullable
                                    if (fieldProperty != null)
                                        fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                                    fieldProperty.PropertyType;

                                    // check if it is a unique id type and not nested object
                                    comboBoxColumn.IsSingle = fieldType.IsSystemType();

                                    // culture
                                    if (binding.ConverterCulture == null)
                                        binding.ConverterCulture = Translate.Culture;
                                }
                                else
                                {
                                    throw new ArgumentException(
                                        "Value of \"SelectedValueBinding\" property or \"SelectedItemBinding\" cannot be null.",
                                        nameof(DataGridComboBoxColumn));
                                }
                                break;

                            case DataGridBoundColumn boundColumn://Is this case neccisary? A DataGridBoundColumn cannot be added in xaml.
                                {
                                    boundColumn.FieldName = ((Binding)boundColumn.Binding).Path.Path;

                                    // template
                                    boundColumn.HeaderTemplate = template;

                                    fieldProperty =
                                        CollectionType.GetProperty(((Binding)boundColumn.Binding).Path.Path);

                                    // get type or underlying type if nullable
                                    if (fieldProperty != null)
                                        fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                                    fieldProperty.PropertyType;

                                    // apply DateFormatString when StringFormat for column is not provided or empty
                                    if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
                                        if (string.IsNullOrEmpty(boundColumn.Binding.StringFormat))
                                            boundColumn.Binding.StringFormat = DateFormatString;

                                    FieldType = fieldType;

                                    // culture
                                    if (((Binding)boundColumn.Binding).ConverterCulture == null)
                                        ((Binding)boundColumn.Binding).ConverterCulture = Translate.Culture;
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratingCustomColumn : {ex.Message}");
                throw;
            }
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
        ///     Reset cursor
        /// </summary>
        private async void ResetCursor()
        {
            // reset cursor
            // Cast Action : compatibility Net4.8
            await Dispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = null; }),
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
                    e.CanExecute = PopupViewItems.Any(f => f?.IsChecked == true);
                else
                    e.CanExecute = PopupViewItems.Any(f => f.IsChanged) &&
                                   PopupViewItems.Any(f => f?.IsChecked == true);
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
        /// Can remove all filter when GlobalFilterList.Count > 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveAllFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalFilterList.Count > 0;
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
            e.CanExecute = CollectionViewSource?.CanFilter == true && (!popup?.IsOpen ?? true) && !currentlyFiltering;
        }

        /// <summary>
        ///     Check/uncheck all item when the action is (select all)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            FilterItem item = (FilterItem)e.Parameter;

            // only when the item[0] (select all) is checked or unchecked
            if (ItemCollectionView == null) return;

            if (item.Level == SelectAllLevel)
            {
                foreach (FilterItem obj in PopupViewItems.Where(f => f.IsChecked != item.IsChecked))
                {
                    obj.IsChecked = item.IsChecked;
                }
            }
            // check if first item select all checkbox (in case of bool?, first item is Unchecked)
            else if (ListBoxItems[0].Level == SelectAllLevel)
            {
                // update select all item status
                ListBoxItems[0].IsChecked = PopupViewItems.All(i => i.IsChecked);
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

            double yAdjust = sizableContentGrid.Height + e.VerticalChange;
            double xAdjust = sizableContentGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or height
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

            Popup pop = (Popup)sender;

            // free the resources if the popup is closed without filtering
            if (!currentlyFiltering)
            {
                CurrentFilter = null;
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());
                ResetCursor();
            }

            // free the resources, unsubscribe from event and re-enable columnHeadersPresenter
            pop.Closed -= PopupClosed;
            pop.MouseDown -= onMousedown;
            searchTextBox.TextChanged -= SearchTextBoxOnTextChanged;
            thumb.DragCompleted -= OnResizeThumbDragCompleted;
            thumb.DragDelta -= OnResizeThumbDragDelta;
            thumb.DragStarted -= OnResizeThumbDragStarted;

            sizableContentGrid.Width = sizableContentWidth;
            sizableContentGrid.Height = sizableContentHeight;
            Cursor = cursor;

            // once the popup is closed, this is no longer necessary
            ListBoxItems = [];
            TreeViewItems = [];

            // re-enable columnHeadersPresenter
            if (columnHeadersPresenter != null)
                columnHeadersPresenter.IsEnabled = true;
        }

        /// <summary>
        ///     Remove All Filter Command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveAllFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveFilters();
        }

        /// <summary>
        ///     Remove current filter
        /// </summary>
        private void RemoveCurrentFilter()
        {
            Debug.WriteLineIf(DebugMode, "RemoveCurrentFilter");

            if (CurrentFilter == null) return;

            popup.IsOpen = false; // raise PopupClosed event

            // reset button icon
            FilterState.SetIsFiltered(CurrentFilter.FilterButton, false);

            ElapsedTime = new TimeSpan(0, 0, 0);
            stopWatchFilter = Stopwatch.StartNew();

            Mouse.OverrideCursor = Cursors.Wait;

            if (CurrentFilter.IsFiltered && criteria.Remove(CurrentFilter.FieldName))
                CollectionViewSource.Refresh();

            if (GlobalFilterList.Contains(CurrentFilter))
                GlobalFilterList.Remove(CurrentFilter);

            // set the last filter applied
            lastFilter = GlobalFilterList.LastOrDefault()?.FieldName;

            CurrentFilter = null;
            ResetCursor();

            if (PersistentFilter)
                SavePreset();

            stopWatchFilter.Stop();
            ElapsedTime = stopWatchFilter.Elapsed;
        }

        /// <summary>
        ///     Remove Current Filter Command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCurrentFilter();
        }

        /// <summary>
        ///     Apply the filter to the items in the popup List/Treeview
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            FilterItem item = (FilterItem)obj;
            if (string.IsNullOrEmpty(searchText) || item == null || item.Level == SelectAllLevel) return true;

            string content = Convert.ToString(item.Content, Translate.Culture);

            // Contains
            if (!StartsWith)
                return Translate.Culture.CompareInfo.IndexOf(content ?? string.Empty, searchText,
                    CompareOptions.OrdinalIgnoreCase) >= 0;

            // StartsWith preserve RangeOverflow
            if (searchLength > item.ContentLength) return false;

            return Translate.Culture.CompareInfo.IndexOf(content ?? string.Empty, searchText, 0, searchLength,
                CompareOptions.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Search TextBox Text Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            TextBox textBox = (TextBox)sender;

            // fix TextChanged event fires twice I did not find another solution
            if (textBox == null || textBox.Text == searchText || ItemCollectionView == null) return;

            searchText = textBox.Text;

            searchLength = searchText.Length;

            search = !string.IsNullOrEmpty(searchText);

            // apply filter (call the SearchFilter method)
            ItemCollectionView.Refresh();

            if (CurrentFilter.FieldType != typeof(DateTime) || treeView == null) return;

            // rebuild treeView
            if (string.IsNullOrEmpty(searchText))
            {
                // populate the tree with items from the source list
                TreeViewItems = await BuildTreeAsync(SourcePopupViewItems);
            }
            else
            {
                // searchText is not empty
                // populate the tree only with items found by the search
                List<FilterItem> items = [.. PopupViewItems.Where(i => i.IsChecked)];

                // if at least one element is not null, fill the tree, otherwise the tree contains only the element (select all).
                TreeViewItems = await BuildTreeAsync(items.Any() ? items : null);
            }
        }

        /// <summary>
        ///     Open a pop-up window, Click on the header button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nShowFilterCommand");

            // reset previous elapsed time
            ElapsedTime = new TimeSpan(0, 0, 0);
            stopWatchFilter = Stopwatch.StartNew();

            // clear search text (!important)
            searchText = string.Empty;
            search = false;

            try
            {
                // filter button
                button = (Button)e.OriginalSource;

                if (Items.Count == 0 || button == null) return;

                // contribution : OTTOSSON
                // for the moment this functionality is not tested, I do not know if it can cause unexpected effects
                _ = CommitEdit(DataGridEditingUnit.Row, true);

                // navigate up to the current header and get column type
                DataGridColumnHeader header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                DataGridColumn headerColumn = header.Column;

                // then down to the current popup
                popup = VisualTreeHelpers.FindChild<Popup>(header, FilterPopupKey);
                columnHeadersPresenter = VisualTreeHelpers.FindAncestor<DataGridColumnHeadersPresenter>(header);

                if (popup == null || columnHeadersPresenter == null) return;

                // disable columnHeadersPresenter while popup is open
                if (columnHeadersPresenter != null)
                    columnHeadersPresenter.IsEnabled = false;

                // popup handle event
                popup.Closed += PopupClosed;

                // disable popup background click-through, contribution : WORDIBOI
                popup.MouseDown += onMousedown;

                // resizable grid
                sizableContentGrid = VisualTreeHelpers.FindChild<Grid>(popup.Child, SizableContentGridKey);

                // search textbox
                searchTextBox = VisualTreeHelpers.FindChild<TextBox>(popup.Child, SearchBoxKey);
                searchTextBox.Text = string.Empty;
                searchTextBox.TextChanged += SearchTextBoxOnTextChanged;
                searchTextBox.Focusable = true;

                // thumb resize grip
                thumb = VisualTreeHelpers.FindChild<Thumb>(sizableContentGrid, PopupThumbKey);

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

                List<FilterItem> filterItemList = null;

                // get field name from binding Path
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (headerColumn is IDataGridColumn col)//Use new interface to set fieldName
                    fieldName = col.FieldName;

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                DataGridComboBoxColumn comboxColumn = null;
                if (headerColumn is DataGridComboBoxColumn comboBoxColumn)
                    comboxColumn = comboBoxColumn;
                // see Extensions helper for GetPropertyInfo
                PropertyInfo fieldProperty = CollectionType.GetPropertyInfo(fieldName);

                // get type or underlying type if nullable
                if (fieldProperty != null)
                    FieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;

                // If no filter, add filter to GlobalFilterList list
                CurrentFilter = GlobalFilterList.FirstOrDefault(f => f.FieldName == fieldName) ??
                                new FilterCommon
                                {
                                    FieldName = fieldName,
                                    FieldType = fieldType,
                                    Translate = Translate,
                                    FilterButton = button
                                };

                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                // contribution : STEFAN HEIMEL
                await Dispatcher.InvokeAsync(() =>
                {
                    // list for all items values, filtered and unfiltered (previous filtered items)
                    List<object> sourceObjectList;

                    // remove NewItemPlaceholder added by DataGrid when user can add row
                    // !System.Windows.Data.CollectionView.NewItemPlaceholder.Equals(x): Explicitly excludes the new entry row.
                    // Testing on DataGridRow is an additional safety feature, but the most important thing is to exclude the NewItemPlaceholder.

                    // get the list of raw values of the current column
                    if (fieldType == typeof(DateTime))
                    {
                        // possible distinct values because time part is removed
                        sourceObjectList = [.. Items.Cast<object>()
                            .Where(x => x is not DataGridRow && !CollectionView.NewItemPlaceholder.Equals(x))
                            .Select(x => (object)((DateTime?)x.GetPropertyValue(fieldName))?.Date)
                            .Distinct()];
                    }
                    else
                    {
                        sourceObjectList = [.. Items.Cast<object>()
                            .Where(x => x is not DataGridRow && !CollectionView.NewItemPlaceholder.Equals(x))
                            .Select(x => x.GetPropertyValue(fieldName))
                            .Distinct()];
                    }

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    if (lastFilter == CurrentFilter.FieldName)
                    {
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems ?? []);
                    }

                    // empty item flag
                    // if they exist, remove all null or empty string values from the list.
                    // content == null and content == string.Empty are two different things but both labeled as (blank)
                    bool emptyItem = sourceObjectList.RemoveAll(v => v == null || v.Equals(string.Empty)) > 0;

                    // TODO : AggregateException when user can add row

                    // sorting is a very slow operation, using ParallelQuery
                    sourceObjectList = sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    if (fieldType == typeof(bool))
                    {
                        filterItemList = new List<FilterItem>(sourceObjectList.Count + 1);
                    }
                    else
                    {
                        // add the first element (select all) at the top of list
                        filterItemList = new List<FilterItem>(sourceObjectList.Count + 2)
                        {
                            // contribution : damonpkuml
                            new() { Label = Translate.All, IsChecked = CurrentFilter?.PreviouslyFilteredItems.Count==0, Level = SelectAllLevel }
                        };
                    }

                    // add all items (not null) to the filterItemList,
                    // the list of dates is calculated by BuildTree from this list
                    filterItemList.AddRange(sourceObjectList.Select(item => new FilterItem
                    {
                        Content = item,
                        ContentLength = item?.ToString().Length ?? 0,
                        FieldType = fieldType,
                        Label = GetLabel(item, fieldType),
                        Level = StandardItemLevel,
                        Initialize = CurrentFilter.PreviouslyFilteredItems?.Contains(item) == false
                    }));

                    // add a empty item(if exist) at the bottom of the list
                    if (emptyItem)
                    {
                        sourceObjectList.Insert(sourceObjectList.Count, null);

                        filterItemList.Add(new FilterItem
                        {
                            FieldType = fieldType,
                            Content = null,
                            Label = fieldType == typeof(bool) ? Translate.Indeterminate : Translate.Empty,
                            Level = EmptyItemLevel,
                            Initialize = CurrentFilter?.PreviouslyFilteredItems?.Contains(null) == false
                        });
                    }

                    string GetLabel(object o, Type type)
                    {
                        // retrieve the label of the list previously reconstituted from "ItemsSource" of the combobox
                        if (comboxColumn?.IsSingle == true)
                        {
                            return comboxColumn.ComboBoxItemsSource
                                ?.FirstOrDefault(x => x.SelectedValue == o.ToString())?.DisplayMember;
                        }

                        // label of other columns
                        return type != typeof(bool) ? o.ToString()
                            // translates boolean value label
                            : o != null && (bool)o ? Translate.IsTrue : Translate.IsFalse;
                    }
                }); // Dispatcher

                // ItemsSource (ListBow/TreeView)
                if (fieldType == typeof(DateTime))
                {
                    TreeViewItems = await BuildTreeAsync(filterItemList);
                }
                else
                {
                    ListBoxItems = filterItemList;
                }

                // Set ICollectionView for filtering in the pop-up window
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(filterItemList);

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
                Debug.WriteLine($"ShowFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // reset cursor
                ResetCursor();

                stopWatchFilter.Stop();

                // show open popup elapsed time in UI
                ElapsedTime = stopWatchFilter.Elapsed;

                Debug.WriteLineIf(DebugMode,
                    $"ShowFilterCommand Elapsed time : {ElapsedTime:mm\\:ss\\.ff}");
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

            stopWatchFilter.Start();

            currentlyFiltering = true;
            popup.IsOpen = false; // raise PopupClosed event

            // set cursor wait
            Mouse.OverrideCursor = Cursors.Wait;

            // Capture current count before filtering
            int previousFilteredCount = FilteredItemsCount;

            try
            {
                await Task.Run(() =>
                {
                    HashSet<object> previousFiltered = CurrentFilter.PreviouslyFilteredItems;
                    FilterItem blankIsChanged = new();

                    if (search)
                    {
                        // in the search, the item (blank) is always unchecked
                        blankIsChanged.IsChecked = false;
                        blankIsChanged.IsChanged = !previousFiltered.Any(c => c != null && c.Equals(string.Empty));

                        // result of the research
                        List<FilterItem> searchResult = [.. PopupViewItems.Where(c => c.IsChecked)];

                        // unchecked : all items except searchResult
                        List<FilterItem> uncheckedItems = [.. SourcePopupViewItems.Except(searchResult)];
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered.ExceptWith(searchResult.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));
                    }
                    else
                    {
                        // changed popup items
                        List<FilterItem> changedItems = [.. PopupViewItems.Where(c => c.IsChanged)];

                        IEnumerable<FilterItem> checkedItems = changedItems.Where(c => c.IsChecked);
                        List<FilterItem> uncheckedItems = [.. changedItems.Where(c => !c.IsChecked)];

                        // previous item except unchecked items checked again
                        previousFiltered.ExceptWith(checkedItems.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsChanged.IsChecked = changedItems.Any(c => c.Level == EmptyItemLevel && c.IsChecked);
                        blankIsChanged.IsChanged = changedItems.Any(c => c.Level == EmptyItemLevel);
                    }

                    if (blankIsChanged.IsChanged && CurrentFilter.FieldType == typeof(string))
                    {
                        // two values: null and string.empty

                        // at this step, the null value is already added previously by the
                        // ShowFilterCommand method

                        switch (blankIsChanged.IsChecked)
                        {
                            // if (blank) item is unchecked, add string.Empty.
                            case false:
                                previousFiltered.Add(string.Empty);
                                break;

                            // if (blank) item is rechecked, remove string.Empty.
                            case true when previousFiltered.Any(c => c?.ToString() == string.Empty):
                                previousFiltered.RemoveWhere(item => item?.ToString() == string.Empty);
                                break;
                        }
                    }

                    // add a filter if it is not already added previously
                    if (!CurrentFilter.IsFiltered) CurrentFilter.AddFilter(criteria);

                    // add current filter to GlobalFilterList
                    if (GlobalFilterList.All(f => f.FieldName != CurrentFilter.FieldName))
                        GlobalFilterList.Add(CurrentFilter);

                    // set the current field name as the last filter name (needed for ShowFilterCommand to reconstruct full list)
                    lastFilter = CurrentFilter.FieldName;
                });

                // Raise FilteredItemsChanging before applying the filter
                OnFilteredItemsChanging(new FilteredItemsChangingEventArgs(previousFilteredCount, -1)); // -1 = unknown upcoming count

                // apply filter
                CollectionViewSource.Refresh();

                // Raise FilteredItemsChanged after applying the filter
                int currentFilteredCount = FilteredItemsCount;
                OnFilteredItemsChanged(new FilteredItemsChangedEventArgs(previousFilteredCount, currentFilteredCount));

                // set button icon (filtered or not)
                FilterState.SetIsFiltered(CurrentFilter.FilterButton, CurrentFilter?.IsFiltered ?? false);

                // remove the current filter if there is no items to filter
                if (CurrentFilter != null && !CurrentFilter.PreviouslyFilteredItems.Any())
                    RemoveCurrentFilter();
                else if (PersistentFilter) // call serialize (if persistent filter)
                    Serialize();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // free resources (unsubscribe from the event and re-enable "columnHeadersPresenter"
                // is done in PopupClosed method)
                currentlyFiltering = false;
                CurrentFilter = null;
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());
                ResetCursor();

                stopWatchFilter.Stop();
                ElapsedTime = stopWatchFilter.Elapsed;

                Debug.WriteLineIf(DebugMode, $@"ApplyFilterCommand Elapsed time : {ElapsedTime:mm\:ss\.ff}");
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

                // get the host window of the datagrid, contribution : STEFAN HEIMEL
                Window hostingWindow = Window.GetWindow(this);

                if (hostingWindow == null) return;

                // get the ContentPresenter from the hostingWindow
                ContentPresenter contentPresenter = VisualTreeHelpers.FindChild<ContentPresenter>(hostingWindow);

                Point hostSize = new()
                {
                    X = contentPresenter.ActualWidth,
                    Y = contentPresenter.ActualHeight
                };

                // get the X, Y position of the header
                Point headerContentOrigin = header.TransformToVisual(contentPresenter).Transform(new Point(0, 0));
                Point headerDataGridOrigin = header.TransformToVisual(this).Transform(new Point(0, 0));

                Point headerSize = new() { X = header.ActualWidth, Y = header.ActualHeight };
                double offset = popUpSize.X - headerSize.X + BorderThickness;

                // the popup must stay in the DataGrid, move it to the left of the header, because it overflows on the right.
                if (headerDataGridOrigin.X + headerSize.X > popUpSize.X) popup.HorizontalOffset -= offset;

                // delta for max size popup
                Point delta = new()
                {
                    X = hostSize.X - (headerContentOrigin.X + headerSize.X),
                    Y = hostSize.Y - (headerContentOrigin.Y + headerSize.Y + popUpSize.Y)
                };
                // max size
                grid.MaxWidth = MaxSize(popUpSize.X + delta.X - BorderThickness);
                grid.MaxHeight = MaxSize(popUpSize.Y + delta.Y - BorderThickness);

                // remove offset
                // contributing to the fix : VASHBALDEUS
                if (popup.HorizontalOffset == 0)
                    grid.MaxWidth = MaxSize(Math.Abs(grid.MaxWidth - offset));

                if (!(delta.Y <= 0d)) return;
                // the height of popup is too large, reduce it, because it overflows down.
                grid.MaxHeight = MaxSize(popUpSize.Y - Math.Abs(delta.Y) - BorderThickness);
                grid.Height = grid.MaxHeight;

                // contributing to the fix : VASHBALDEUS
                grid.MinHeight = grid.MaxHeight == 0 ? grid.MinHeight : grid.MaxHeight;

                // greater than or equal to 0.0
                static double MaxSize(double size)
                {
                    return size >= 0.0d ? size : 0.0d;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PopupPlacement error : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Renumber all rows when ItemsSource uses ObservableCollection
        ///     which implements INotifyCollectionChanged
        ///     Contribution : mcboothy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "ItemSourceCollectionChanged");

            ItemsSourceCount = Items.Count;
            OnPropertyChanged(nameof(ItemsSourceCount));

            if (!ShowRowsCount) return;
            // Renumber all rows
            for (int i = 0; i < Items.Count; i++)
                if (ItemContainerGenerator.ContainerFromIndex(i) is DataGridRow row)
                    row.Header = $"{i + 1}";
        }

        /// <summary>
        /// Raises the ItemsSourceChanging event
        /// </summary>
        private void OnItemsSourceChanging(ItemsSourceChangingEventArgs e)
        {
            ItemsSourceChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ItemsSourceChanged event
        /// </summary>
        private void OnItemsSourceChanged(ItemsSourceChangedEventArgs e)
        {
            ItemsSourceChanged?.Invoke(this, e);

            // Raise unified DataChanged event
            OnDataChanged();
        }

        /// <summary>
        /// Raised before filtered items are about to change (before filter is applied/removed)
        /// Provides current and upcoming filtered item counts
        /// </summary>
        private void OnFilteredItemsChanging(FilteredItemsChangingEventArgs e)
        {
            FilteredItemsChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Raised after filtered items have changed (after filter is applied/removed)
        /// Provides previous and current filtered item counts
        /// </summary>
        private void OnFilteredItemsChanged(FilteredItemsChangedEventArgs e)
        {
            FilteredItemsChanged?.Invoke(this, e);

            // Raise unified DataChanged event
            OnDataChanged();
        }

        /// <summary>
        /// Raises the unified DataChanged event
        /// Fired whenever ItemsSource or filtered items change
        /// </summary>
        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }

        /// <summary>
        /// Captures current state before ItemsSource change
        /// </summary>
        private void CaptureCurrentState()
        {
            // Capture selected item/index
            selectedItemBeforeChange = SelectedItem;
            selectedIndexBeforeChange = SelectedIndex;

            // Capture scroll position
            ScrollViewer scrollViewer = VisualTreeHelpers.FindChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                verticalOffsetBeforeChange = scrollViewer.VerticalOffset;
                horizontalOffsetBeforeChange = scrollViewer.HorizontalOffset;
            }

            // Capture filter state
            filterStateBeforeChange = [];
            IReadOnlyList<FilterCommon> activeFilters = ActiveFilters;

            foreach (FilterCommon filter in activeFilters)
            {
                FilterCommon filterCopy = new()
                {
                    FieldName = filter.FieldName,
                    FieldType = filter.FieldType,
                    PreviouslyFilteredItems = [.. filter.PreviouslyFilteredItems]
                };
                filterStateBeforeChange.Add(filterCopy);
            }
        }

        /// <summary>
        /// Restores state after ItemsSource change
        /// </summary>
        private void RestoreState()
        {
            // Restore filters
            if (filterStateBeforeChange != null && filterStateBeforeChange.Count > 0)
            {
                ApplyFiltersInternal(filterStateBeforeChange);
            }

            // Restore selected item
            if (selectedItemBeforeChange != null)
            {
                SelectedItem = Items.Cast<object>().FirstOrDefault(item =>
                    FilterDataGrid.ItemsAreEqual(item, selectedItemBeforeChange));

                if (SelectedItem == null && selectedIndexBeforeChange >= 0 && selectedIndexBeforeChange < Items.Count)
                {
                    SelectedIndex = selectedIndexBeforeChange;
                }
            }

            // Restore scroll position
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ScrollViewer scrollViewer = VisualTreeHelpers.FindChild<ScrollViewer>(this);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(verticalOffsetBeforeChange);
                    scrollViewer.ScrollToHorizontalOffset(horizontalOffsetBeforeChange);
                }
            }), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Applies filters internally without using reflection (direct access to private members)
        /// </summary>
        private void ApplyFiltersInternal(List<FilterCommon> filters)
        {
            if (filters == null || filters.Count == 0) return;

            // Capture current count before filtering
            int previousFilteredCount = FilteredItemsCount;

            // Raise FilteredItemsChanging before applying the filter
            OnFilteredItemsChanging(new FilteredItemsChangingEventArgs(previousFilteredCount, -1)); // -1 = unknown upcoming count

            foreach (FilterCommon filter in filters)
            {
                DataGridColumn column = Columns.FirstOrDefault(c =>
                    (c is IDataGridColumn dgc && dgc.FieldName == filter.FieldName));

                if (column == null) continue;

                Button filterButton = VisualTreeHelpers.GetHeader(column, this)
                    ?.FindVisualChild<Button>(FilterButtonKey);

                filter.FilterButton = filterButton;
                filter.Translate = Translate;

                if (!filter.IsFiltered)
                {
                    filter.AddFilter(criteria);
                }

                // add current filter to GlobalFilterList
                if (GlobalFilterList.All(f => f.FieldName != filter.FieldName))
                    GlobalFilterList.Add(filter);

                if (filterButton != null)
                {
                    FilterState.SetIsFiltered(filterButton, true);
                }

                // set the current field name as the last filter name (needed for ShowFilterCommand to reconstruct full list)
                lastFilter = filter.FieldName;
            }

            RefreshFilter();

            // Raise FilteredItemsChanged after applying filters
            int currentFilteredCount = FilteredItemsCount;
            OnFilteredItemsChanged(new FilteredItemsChangedEventArgs(previousFilteredCount, currentFilteredCount));

            OnDataChanged();
        }

        /// <summary>
        /// Checks if two items are equal for selection restoration
        /// </summary>
        private static bool ItemsAreEqual(object item1, object item2)
        {
            if (item1 == null || item2 == null) return false;
            if (item1.Equals(item2)) return true;

            // Try to compare by key properties if available
            Type itemType = item1.GetType();
            PropertyInfo[] properties = itemType.GetProperties();

            PropertyInfo idProperty = properties.FirstOrDefault(p =>
                p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals($"{itemType.Name}Id", StringComparison.OrdinalIgnoreCase));

            if (idProperty != null)
            {
                object id1 = idProperty.GetValue(item1);
                object id2 = idProperty.GetValue(item2);
                return id1 != null && id1.Equals(id2);
            }

            return false;
        }
    }

    #endregion Private Methods

    #region Event Args Classes

    /// <summary>
    /// Event arguments for items source changing
    /// </summary>
    public class ItemsSourceChangingEventArgs(IEnumerable oldSource, IEnumerable newSource) : EventArgs
    {
        public IEnumerable OldSource { get; } = oldSource;
        public IEnumerable NewSource { get; } = newSource;
        public bool Cancel { get; set; } = false;
    }

    /// <summary>
    /// Event arguments for items source changed complete
    /// </summary>
    public class ItemsSourceChangedEventArgs(IEnumerable oldSource, IEnumerable newSource, bool stateRestored) : EventArgs
    {
        public IEnumerable OldSource { get; } = oldSource;
        public IEnumerable NewSource { get; } = newSource;
        public bool StateRestored { get; } = stateRestored;
    }

    /// <summary>
    /// Event arguments for filtered items changing
    /// </summary>
    public class FilteredItemsChangingEventArgs(int currentFilteredItemsCount, int upcomingFilteredItemsCount) : EventArgs
    {
        public int CurrentFilteredItemsCount { get; } = currentFilteredItemsCount;
        public int UpcomingFilteredItemsCount { get; } = upcomingFilteredItemsCount;
    }

    /// <summary>
    /// Event arguments for filtered items changed
    /// </summary>
    public class FilteredItemsChangedEventArgs(int previousFilteredItemsCount, int currentFilteredItemsCount) : EventArgs
    {
        public int PreviousFilteredItemsCount { get; } = previousFilteredItemsCount;
        public int CurrentFilteredItemsCount { get; } = currentFilteredItemsCount;
    }

    /// <summary>
    /// Event arguments for data changes
    /// Raised whenever ItemsSource or filtered items change
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        public DateTime Timestamp { get; } = DateTime.Now;
    }

    #endregion Event Args Classes

    #region Filter Data Classes

    /// <summary>
    /// Serializable filter state data
    /// </summary>
    [Serializable]
    public class FilterStateData
    {
        public List<FilterData> Filters { get; set; }
    }

    /// <summary>
    /// Serializable filter data
    /// </summary>
    [Serializable]
    public class FilterData
    {
        public string FieldName { get; set; }
        public string FieldTypeName { get; set; }
        public List<object> FilteredItems { get; set; }
    }

    #endregion Filter Data Classes

    #region Filter Builder

    /// <summary>
    /// Builder class for programmatic filter construction
    /// </summary>
    public class FilterBuilder(FilterDataGrid grid)
    {
        private readonly List<FilterData> filters = [];

        /// <summary>
        /// Adds a filter for a specific column
        /// </summary>
        /// <param name="fieldName">The field name to filter</param>
        /// <param name="excludedValues">Values to exclude from the view</param>
        /// <returns>The filter builder for chaining</returns>
        public FilterBuilder AddFilter(string fieldName, params object[] excludedValues)
        {
            DataGridColumn column = grid.Columns.FirstOrDefault(c =>
                c is IDataGridColumn dgc && dgc.FieldName == fieldName) ?? throw new ArgumentException($"Column with field name '{fieldName}' not found.", nameof(fieldName));
            PropertyInfo fieldProperty = grid.CollectionType?.GetPropertyInfo(fieldName);

            fieldProperty ??= grid.ItemsSource.Cast<object>().First().GetType().GetPropertyInfo(fieldName);

            Type fieldType = null;

            if (fieldProperty != null)
            {
                fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
            }

            if (fieldType == null)
            {
                throw new InvalidOperationException($"Cannot determine field type for '{fieldName}'.");
            }

            FilterData filterData = new()
            {
                FieldName = fieldName,
                FieldTypeName = fieldType.AssemblyQualifiedName,
                FilteredItems = [.. excludedValues]
            };

            filters.Add(filterData);
            return this;
        }

        /// <summary>
        /// Adds a filter to include only specific values
        /// </summary>
        /// <param name="fieldName">The field name to filter</param>
        /// <param name="includedValues">Values to include in the view</param>
        /// <returns>The filter builder for chaining</returns>
        public FilterBuilder AddIncludeFilter(string fieldName, List<object> includedValues)
        {
            // Get all distinct values for the field
            List<object> allValues = [.. grid.Items.Cast<object>()
                .Select(item => item.GetPropertyValue(fieldName))
                .Distinct()];

            // Exclude everything except included values
            List<object> excludedValues = [.. allValues.Except(includedValues)];

            return AddFilter(fieldName, [.. excludedValues]);
        }

        /// <summary>
        /// Applies all filters built with this builder
        /// </summary>
        public void Apply()
        {
            FilterStateData stateData = new()
            {
                Filters = filters
            };

            grid.ApplyFilterState(stateData);
        }

        /// <summary>
        /// Clears all filters from the builder
        /// </summary>
        /// <returns>The filter builder for chaining</returns>
        public FilterBuilder Clear()
        {
            filters.Clear();
            return this;
        }
    }

    #endregion Filter Builder
}