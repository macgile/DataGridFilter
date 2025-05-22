#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : FilterDataGrid.Net
// File       : FilterDataGrid.cs
// Created    : 06/03/2022
//

#endregion

using FilterDataGrid.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/FilterDataGrid;component/Themes/Generic.xaml", UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(resourceDictionary);

            // initial popup size
            popUpSize = new Point
            {
                X = (double)TryFindResource("PopupWidth"),
                Y = (double)TryFindResource("PopupHeight")
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
                new PropertyMetadata(""));

        /// <summary>
        ///     Excluded Column (only AutoGeneratingColumn)
        /// </summary>
        public static readonly DependencyProperty ExcludeColumnsProperty =
            DependencyProperty.Register("ExcludeColumns",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(""));

        /// <summary>
        ///     Date format displayed
        /// </summary>
        public static readonly DependencyProperty DateFormatStringProperty =
            DependencyProperty.Register("DateFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata("d"));
        /// <summary>
        ///     Time format displayed
        /// </summary>
        public static readonly DependencyProperty TimeFormatStringProperty =
            DependencyProperty.Register("TimeFormatString",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(null));


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

        #endregion Public Event

        #region Private Fields

        private const bool DebugMode = true;

        private string fileName = "persistentFilter.json";
        private Stopwatch stopWatchFilter = new Stopwatch();
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

        private Type collectionType;
        private Type fieldType;

        private bool startsWith;

        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();

        #endregion Private Fields

        #region Public Properties

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
        ///     Date format displayed
        /// </summary>
        public string TimeFormatString
        {
            get => (string)GetValue(TimeFormatStringProperty);
            set => SetValue(TimeFormatStringProperty, value);
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
            get => treeView ?? new List<FilterItemDate>();
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
            get => listBoxItems ?? new List<FilterItem>();
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

        #endregion Public Properties

        #region Private Properties

        private FilterCommon CurrentFilter { get; set; }
        private ICollectionView CollectionViewSource { get; set; }
        private ICollectionView ItemCollectionView { get; set; }
        private List<FilterCommon> GlobalFilterList { get; } = new List<FilterCommon>();

        /// <summary>
        /// Popup filtered items (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> PopupViewItems =>
            ItemCollectionView?.OfType<FilterItem>().Where(c => c.Level != 0) ?? new List<FilterItem>();

        /// <summary>
        /// Popup source collection (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> SourcePopupViewItems =>
            ItemCollectionView?.SourceCollection.OfType<FilterItem>().Where(c => c.Level != 0) ??
            new List<FilterItem>();

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
                    excludedFields = ExcludeFields.Split(',').Select(p => p.Trim()).ToList();
                    excludedColumns = ExcludeColumns.Split(',').Select(p => p.Trim()).ToList();
                }
                // generating custom columns
                else if (collectionType != null) GeneratingCustomsColumn();

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
                if (excludedColumns.Any(
                        x => string.Equals(x, e.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    e.Cancel = true;
                    return;
                }

                // enable column sorting when user specified
                e.Column.CanUserSort = CanUserSortColumns;

                // return if the field is excluded
                if (excludedFields.Any(c =>
                        string.Equals(c, e.PropertyName, StringComparison.CurrentCultureIgnoreCase))) return;

                // template
                var template = (DataTemplate)TryFindResource("DataGridHeaderTemplate");

                // get type
                fieldType = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                // get type code
                var typeCode = Type.GetTypeCode(fieldType);

                if (fieldType.IsEnum)
                {
                    var column = new DataGridComboBoxColumn
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
                    var column = new DataGridCheckBoxColumn
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
                else if ((int)typeCode > 4 && (int)typeCode < 16)
                {
                    var column = new DataGridNumericColumn()
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
                    var column = new DataGridTextColumn
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header,
                        IsColumnFiltered = true
                    };

                    // apply the format string provided
                    if (fieldType.IsDate() && !string.IsNullOrEmpty(DateFormatString))
                        column.Binding.StringFormat = DateFormatString;
                    if (fieldType.IsTime() && !string.IsNullOrEmpty(TimeFormatString))
                        column.Binding.StringFormat = TimeFormatString;

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
                    foreach (var col in Columns)
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
                    var scrollViewer = GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
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
                    var txt = new TextBlock
                    {
                        Text = ItemsSourceCount.ToString(),
                        FontSize = FontSize,
                        FontFamily = FontFamily,
                        Padding = new Thickness(0, 0, 4, 0),
                        Margin = new Thickness(2.0)
                    };
                    txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    RowHeaderWidth = Math.Max(Math.Ceiling(txt.DesiredSize.Width),
                        RowHeaderWidth >= 0 ? RowHeaderWidth : 0);
                }
                else
                {
                    // default value, if this value is set to 0, the row header is not displayed
                    // and the exception occurs when the value is set to < 0
                    RowHeaderWidth = 6;
                }

                // get collection type
                // contribution : APFLKUACHA
                collectionType = ItemsSource is ICollectionView collectionView
                    ? collectionView.SourceCollection?.GetType().GenericTypeArguments.FirstOrDefault()
                    : ItemsSource?.GetType().GenericTypeArguments.FirstOrDefault();

                // set name of persistent filter json file
                // The name of the file is defined by the "Name" property of the FilterDatGrid, otherwise
                // the name of the source collection type is used
                if (collectionType != null)
                    fileName = !string.IsNullOrEmpty(Name) ? $"{Name}.json" : $"{collectionType?.Name}.json";

                // generating custom columns
                if (!AutoGenerateColumns && collectionType != null) GeneratingCustomsColumn();

                // re-evalutate the command's CanExecute.
                // when "IsReadOnly" is set to "False", "CanRemoveAllFilter" is not re-evaluated,
                // the "Remove All Filters" icon remains active
                CommandManager.InvalidateRequerySuggested();
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
            if(ShowRowsCount)
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

            try
            {
                foreach (var filterButton in GlobalFilterList.Select(filter => filter.FilterButton))
                {
                    FilterState.SetIsFiltered(filterButton, false);
                }

                // reset current filter
                CurrentFilter = null;
                criteria.Clear();
                GlobalFilterList.Clear();
                CollectionViewSource?.Refresh();

                // empty json file
                if (PersistentFilter) SavePreset();
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugMode, $"RemoveFilters error : {ex.Message}");
                throw;
            }
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

            var hostingWindow = Window.GetWindow(this);

            // set the background color of the filter popup
            FilterPopupBackground = FilterPopupBackground == null && hostingWindow != null
                ? hostingWindow.Background
                : new SolidColorBrush(Colors.White);

            if(filterDataGrid.PersistentFilter)
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
                foreach (var preset in filterPreset)
                {
                    // Get columns that match the preset field name and are filterable
                    var columns = Columns
                        .Where(c =>
                            (c is DataGridTextColumn dtx && dtx.IsColumnFiltered && dtx.FieldName == preset.FieldName)
                            || (c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered && dtp.FieldName == preset.FieldName)
                            || (c is DataGridCheckBoxColumn dck && dck.IsColumnFiltered && dck.FieldName == preset.FieldName)
                            || (c is DataGridNumericColumn dnm && dnm.IsColumnFiltered && dnm.FieldName == preset.FieldName)
                            || (c is DataGridComboBoxColumn cmb && cmb.IsColumnFiltered && cmb.FieldName == preset.FieldName))
                        .ToList();

                    foreach (var col in columns)
                    {
                        // Get distinct values from the ItemsSource for the current column
                        var sourceObjectList = Items.Distincts(preset.FieldType, preset.FieldName);

                        // Convert previously filtered items to the correct type
                        preset.PreviouslyFilteredItems = preset.PreviouslyFilteredItems
                            .Select(o => o.ConvertToType(preset.FieldType))
                            .ToHashSet();

                        // Get the items that are always present in the source collection
                        preset.FilteredItems = sourceObjectList
                            .Where(c => preset.PreviouslyFilteredItems.Contains(c))
                            .ToList();

                        // if no items are filtered, continue to the next column
                        if (preset.FilteredItems.Count == 0)
                            continue;

                        preset.Translate = Translate;

                        var filterButton = VisualTreeHelpers.GetHeader(col, this)
                            ?.FindVisualChild<Button>("FilterButton");

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
        /// Serialize filters list
        /// </summary>
        private async void Serialize()
        {
            await Task.Run(() =>
            {
                var result = JsonConvert.Serialize(fileName, GlobalFilterList);
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
                var result = JsonConvert.Deserialize<List<FilterCommon>>(fileName);

                if (result == null) return;
                Dispatcher.BeginInvoke((Action)(() => { OnFilterPresetChanged(result); }),
                    DispatcherPriority.Normal);

                Debug.WriteLineIf(DebugMode, $"DeSerialize : {result.Count}");
            });
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
                // get the columns that can be filtered
                // ReSharper disable MergeIntoPattern
                var columns = Columns
                    .Where(c => ((c is DataGridBoundColumn dbu && dbu.IsColumnFiltered)
                                  || (c is DataGridCheckBoxColumn dcb && dcb.IsColumnFiltered)
                                  || (c is DataGridComboBoxColumn dbx && dbx.IsColumnFiltered)
                                  || (c is DataGridNumericColumn dnm && dnm.IsColumnFiltered)
                                  || (c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered)
                                  || (c is DataGridTextColumn dtx && dtx.IsColumnFiltered))
                    )
                    .Select(c => c)
                    .ToList();

                // set header template
                foreach (var col in columns)
                {
                    var columnType = col.GetType();

                    if (col.HeaderTemplate != null)
                    {
                        // Debug.WriteLineIf(DebugMode, "\tReset filter Button");

                        // reset filter Button
                        var buttonFilter = VisualTreeHelpers.GetHeader(col, this)
                            ?.FindVisualChild<Button>("FilterButton");

                        if (buttonFilter != null) FilterState.SetIsFiltered(buttonFilter, false);

                        // update the "ComboBoxItemsSource" custom property of "DataGridComboBoxColumn"
                        // this collection may change when loading a new source collection of the DataGrid.
                        if (columnType == typeof(DataGridComboBoxColumn))
                        {
                            var comboBoxColumn = (DataGridComboBoxColumn)col;
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
                        var template = (DataTemplate)TryFindResource("DataGridHeaderTemplate");

                        if (columnType == typeof(DataGridTemplateColumn))
                        {
                            // DataGridTemplateColumn has no culture property
                            var column = (DataGridTemplateColumn)col;

                            if (string.IsNullOrEmpty(column.FieldName))
                                throw new ArgumentException("Value of \"FieldName\" property cannot be null.",
                                    nameof(DataGridTemplateColumn));
                            // template
                            column.HeaderTemplate = template;
                        }

                        if (columnType == typeof(DataGridBoundColumn))
                        {
                            var column = (DataGridBoundColumn)col;

                            column.FieldName = ((Binding)column.Binding).Path.Path;

                            // template
                            column.HeaderTemplate = template;

                            var fieldProperty = collectionType.GetProperty(((Binding)column.Binding).Path.Path);

                            // get type or underlying type if nullable
                            if (fieldProperty != null)
                                fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                            fieldProperty.PropertyType;

                            // apply DateFormatString when StringFormat for column is not provided or empty
                            if (fieldType.IsDate() && !string.IsNullOrEmpty(DateFormatString))
                                if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                    column.Binding.StringFormat = DateFormatString;
                            else if (fieldType.IsTime() && !string.IsNullOrEmpty(TimeFormatString))
                                if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                    column.Binding.StringFormat = TimeFormatString;

                            FieldType = fieldType;

                            // culture
                            if (((Binding)column.Binding).ConverterCulture == null)
                                ((Binding)column.Binding).ConverterCulture = Translate.Culture;
                        }

                        if (columnType == typeof(DataGridTextColumn))
                        {
                            var column = (DataGridTextColumn)col;

                            column.FieldName = ((Binding)column.Binding).Path.Path;

                            // template
                            column.HeaderTemplate = template;

                            var fieldProperty = collectionType.GetProperty(((Binding)column.Binding).Path.Path);

                            // get type or underlying type if nullable
                            if (fieldProperty != null)
                                fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                            fieldProperty.PropertyType;

                            // apply DateFormatString when StringFormat for column is not provided or empty
                            if (fieldType.IsDate() && !string.IsNullOrEmpty(DateFormatString))
                                if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                    column.Binding.StringFormat = DateFormatString;
                                else if (fieldType.IsTime() && !string.IsNullOrEmpty(TimeFormatString))
                                    if (string.IsNullOrEmpty(column.Binding.StringFormat))
                                        column.Binding.StringFormat = TimeFormatString;

                            FieldType = fieldType;

                            // culture
                            if (((Binding)column.Binding).ConverterCulture == null)
                                ((Binding)column.Binding).ConverterCulture = Translate.Culture;
                        }

                        if (columnType == typeof(DataGridCheckBoxColumn))
                        {
                            var column = (DataGridCheckBoxColumn)col;

                            column.FieldName = ((Binding)column.Binding).Path.Path;

                            // template
                            column.HeaderTemplate = template;

                            // culture
                            if (((Binding)column.Binding).ConverterCulture == null)
                                ((Binding)column.Binding).ConverterCulture = Translate.Culture;
                        }

                        if (columnType == typeof(DataGridComboBoxColumn))
                        {
                            var column = (DataGridComboBoxColumn)col;

                            if (column.ItemsSource == null) return;

                            var binding = (Binding)column.SelectedValueBinding ?? (Binding)column.SelectedItemBinding;

                            // check if binding is missing
                            if (binding != null)
                            {
                                column.FieldName = binding.Path.Path;

                                // template
                                column.HeaderTemplate = template;

                                var fieldProperty = collectionType.GetPropertyInfo(column.FieldName);

                                // get type or underlying type if nullable
                                if (fieldProperty != null)
                                    fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
                                                fieldProperty.PropertyType;

                                // check if it is a unique id type and not nested object
                                column.IsSingle = fieldType.IsSystemType();

                                // culture
                                if (binding.ConverterCulture == null) binding.ConverterCulture = Translate.Culture;
                            }
                            else
                            {
                                throw new ArgumentException(
                                    "Value of \"SelectedValueBinding\" property or \"SelectedItemBinding\" cannot be null.",
                                    nameof(DataGridComboBoxColumn));
                            }
                        }

                        if (columnType == typeof(DataGridNumericColumn))
                        {
                            var column = (DataGridNumericColumn)col;

                            column.FieldName = ((Binding)column.Binding).Path.Path;

                            // template
                            column.HeaderTemplate = template;

                            // culture
                            if (((Binding)column.Binding).ConverterCulture == null)
                                ((Binding)column.Binding).ConverterCulture = Translate.Culture;
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
            var item = (FilterItem)e.Parameter;

            // only when the item[0] (select all) is checked or unchecked
            if (ItemCollectionView == null) return;

            if (item.Level == 0)
            {
                foreach (var obj in PopupViewItems.Where(f => f.IsChecked != item.IsChecked))
                {
                    obj.IsChecked = item.IsChecked;
                }
            }
            // check if first item select all checkbox (in case of bool?, first item is Unchecked)
            else if(ListBoxItems[0].Level==0)
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

            var yAdjust = sizableContentGrid.Height + e.VerticalChange;
            var xAdjust = sizableContentGrid.Width + e.HorizontalChange;

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

            var pop = (Popup)sender;

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
            ListBoxItems = new List<FilterItem>();
            TreeViewItems = new List<FilterItemDate>();

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
            var item = (FilterItem)obj;
            if (string.IsNullOrEmpty(searchText) || item == null || item.Level == 0) return true;

            var content = Convert.ToString(item.Content, Translate.Culture);

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
            var textBox = (TextBox)sender;

            // fix TextChanged event fires twice I did not find another solution
            if (textBox == null || textBox.Text == searchText || ItemCollectionView == null) return;

            searchText = textBox.Text;

            searchLength = searchText.Length;

            search = !string.IsNullOrEmpty(searchText);

            // apply filter (call the SearchFilter method)
            ItemCollectionView.Refresh();

            if (!CurrentFilter.FieldType.IsDateOrTime() || treeView == null) return;

            // rebuild treeView
            if (string.IsNullOrEmpty(searchText))
            {
                // populate the tree with items from the source list
                TreeViewItems = await SourcePopupViewItems.BuildTreeAsync(Translate, fieldType);
            }
            else
            {
                // searchText is not empty
                // populate the tree only with items found by the search
                var items = PopupViewItems.Where(i => i.IsChecked).ToList();

                // if at least one element is not null, fill the tree, otherwise the tree contains only the element (select all).
                TreeViewItems = await (items.Any() ? items : null).BuildTreeAsync(Translate, fieldType);
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
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                var headerColumn = header.Column;

                // then down to the current popup
                popup = VisualTreeHelpers.FindChild<Popup>(header, "FilterPopup");
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

                List<FilterItem> filterItemList = null;
                DataGridComboBoxColumn comboxColumn = null;

                // get field name from binding Path
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (headerColumn is DataGridTextColumn textColumn)
                {
                    fieldName = textColumn.FieldName;
                }
                if (headerColumn is DataGridBoundColumn templateBound)
                {
                    fieldName = templateBound.FieldName;
                }
                if (headerColumn is DataGridTemplateColumn templateColumn)
                {
                    fieldName = templateColumn.FieldName;
                }
                if (headerColumn is DataGridCheckBoxColumn checkBoxColumn)
                {
                    fieldName = checkBoxColumn.FieldName;
                }
                if (headerColumn is DataGridNumericColumn numericColumn)
                {
                    fieldName = numericColumn.FieldName;
                }
                if (headerColumn is DataGridComboBoxColumn comboBoxColumn)
                {
                    fieldName = comboBoxColumn.FieldName;
                    comboxColumn = comboBoxColumn;
                }

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                // see Extensions helper for GetPropertyInfo
                var fieldProperty = collectionType.GetPropertyInfo(fieldName);

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
                    List<object> sourceObjectList = Items.Distincts(fieldType, fieldName);

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    if (lastFilter == CurrentFilter.FieldName)
                    {
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems ?? new HashSet<object>());
                    }

                    // empty item flag
                    // if they exist, remove all null or empty string values from the list.
                    // content == null and content == "" are two different things but both labeled as (blank)
                    var emptyItem = sourceObjectList.RemoveAll(v => v == null || v.Equals(string.Empty)) > 0;

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
                            new FilterItem { Label = Translate.All, IsChecked = CurrentFilter?.PreviouslyFilteredItems.Count==0, Level = 0 }
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
                        Level = 1,
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
                            Level = -1,
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
                if (fieldType.IsDateOrTime())
                {
                    TreeViewItems = await filterItemList.BuildTreeAsync(Translate, fieldType);
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

            try
            {
                await Task.Run(() =>
                {
                    var previousFiltered = CurrentFilter.PreviouslyFilteredItems;
                    var blankIsChanged = new FilterItem();

                    if (search)
                    {
                        // in the search, the item (blank) is always unchecked
                        blankIsChanged.IsChecked = false;
                        blankIsChanged.IsChanged = !previousFiltered.Any(c => c != null && c.Equals(string.Empty));

                        // result of the research
                        var searchResult = PopupViewItems.Where(c => c.IsChecked).ToList();

                        // unchecked : all items except searchResult
                        var uncheckedItems = SourcePopupViewItems.Except(searchResult).ToList();
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered.ExceptWith(searchResult.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));
                    }
                    else
                    {
                        // changed popup items
                        var changedItems = PopupViewItems.Where(c => c.IsChanged).ToList();

                        var checkedItems = changedItems.Where(c => c.IsChecked);
                        var uncheckedItems = changedItems.Where(c => !c.IsChecked).ToList();

                        // previous item except unchecked items checked again
                        previousFiltered.ExceptWith(checkedItems.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsChanged.IsChecked = changedItems.Any(c => c.Level == -1 && c.IsChecked);
                        blankIsChanged.IsChanged = changedItems.Any(c => c.Level == -1);
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

                    // set the current field name as the last filter name
                    lastFilter = CurrentFilter.FieldName;
                });

                // apply filter
                CollectionViewSource.Refresh();

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
                var hostingWindow = Window.GetWindow(this);

                if (hostingWindow == null) return;

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
                // contributing to the fix : VASHBALDEUS
                if (popup.HorizontalOffset == 0)
                    grid.MaxWidth = MaxSize(Math.Abs(grid.MaxWidth - offset));

                if (!(delta.Y <= 0d)) return;

                // the height of popup is too large, reduce it, because it overflows down.
                grid.MaxHeight = MaxSize(popUpSize.Y - Math.Abs(delta.Y) - border);
                grid.Height = grid.MaxHeight;

                // contributing to the fix : VASHBALDEUS
                grid.MinHeight = grid.MaxHeight == 0 ? grid.MinHeight : grid.MaxHeight;

                // greater than or equal to 0.0
                double MaxSize(double size)
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

            if(!ShowRowsCount) return;
            // Renumber all rows
            for (var i = 0; i < Items.Count; i++)
                if (ItemContainerGenerator.ContainerFromIndex(i) is DataGridRow row)
                    row.Header = $"{i + 1}";
        }

        #endregion Private Methods
    }
}
