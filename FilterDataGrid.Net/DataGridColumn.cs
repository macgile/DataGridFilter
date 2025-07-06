// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : DataGridColumn.cs
// Created    : 09/11/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable ConvertTypeCheckPatternToNullCheck
// ReSharper disable InvertIf
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable once ClassNeverInstantiated.Global

namespace FilterDataGrid
{
    public interface IDataGridColumn
    {
        #region Public Properties

        string FieldName { get; set; }
        bool IsColumnFiltered { get; set; }

        #endregion Public Properties
    }

    public class DataGridBoundColumn : System.Windows.Controls.DataGridBoundColumn, IDataGridColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridTextColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(DataGridTextColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties

        #region GenerateElement

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string TemplateName { get; set; }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem) => GenerateElement(cell, dataItem);

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var content = new ContentControl()
            {
                ContentTemplate = (DataTemplate)cell.FindResource(TemplateName)
            };

            if (Binding != null)
            {
                var binding = new Binding(((Binding)Binding).Path.Path)
                {
                    Source = dataItem,
                    Mode = BindingMode.TwoWay,
                    NotifyOnSourceUpdated = true,
                    NotifyOnTargetUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                content.SetBinding(ContentControl.ContentProperty, binding);
            }

            return content;
        }
        #endregion GenerateElement
    }
    
    public class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn, IDataGridColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridCheckBoxColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(DataGridCheckBoxColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties
    }

    public class DataGridComboBoxColumn : System.Windows.Controls.DataGridComboBoxColumn, IDataGridColumn
    {
        #region Public Classes

        public class ItemsSourceMembers
        {
            #region Public Properties

            public string DisplayMember { get; set; }
            public string SelectedValue { get; set; }

            #endregion Public Properties
        }

        #endregion Public Classes

        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridComboBoxColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(DataGridComboBoxColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public List<ItemsSourceMembers> ComboBoxItemsSource { get; set; }

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        public bool IsSingle { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Updates the items source.
        /// </summary>
        public async void UpdateItemsSourceAsync()
        {
            if (ItemsSource == null) return;

            // Marshal the call back to the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                var itemsSource = ItemsSource;
                var itemsSourceMembers = itemsSource.Cast<object>().Select(x =>
                    new ItemsSourceMembers
                    {
                        SelectedValue = x.GetPropertyValue(SelectedValuePath).ToString(),
                        DisplayMember = x.GetPropertyValue(DisplayMemberPath).ToString()
                    }).ToList();

                ComboBoxItemsSource = itemsSourceMembers;
            });
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnSelectedValueBindingChanged(BindingBase oldBinding, BindingBase newBinding)
        {
            base.OnSelectedValueBindingChanged(oldBinding, newBinding);
            UpdateItemsSourceAsync();
        }

        #endregion Protected Methods
    }

    public class DataGridNumericColumn : DataGridTextColumn
    {
        #region Private Fields

        private const bool DebugMode = false;
        private CultureInfo culture;
        private Type fieldType;
        private string originalValue;
        private Regex regex;
        private string stringFormat;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Determines if the field type is numeric and sets the appropriate regex pattern.
        /// </summary>
        public void BuildRegex()
        {
            Debug.WriteLineIf(DebugMode, $"BuildRegex : {fieldType}");
            var nfi = culture.NumberFormat;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Type.GetTypeCode(fieldType))
            {
                // signed integer types
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                    regex = new Regex($@"^{nfi.NegativeSign}?\d+$");
                    break;

                // unsigned integer types
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    regex = new Regex(@"^\d+$");
                    break;

                // floating point types
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    var decimalSeparator = stringFormat.Contains("c")
                        ? Regex.Escape(nfi.CurrencyDecimalSeparator)
                        : Regex.Escape(nfi.NumberDecimalSeparator);
                    regex = new Regex($@"^{nfi.NegativeSign}?(\d+({decimalSeparator}\d*)?|{decimalSeparator}\d*)?$");
                    break;

                // non-numeric types
                default:
                    Debug.WriteLineIf(DebugMode, "Unsupported fieldType");
                    regex = new Regex(@"[^\t\r\n]+");
                    break;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Cancels the cell edit.
        /// </summary>
        /// <param name="editingElement">The editing element.</param>
        /// <param name="uneditedValue">The unedited value.</param>
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            Debug.WriteLineIf(DebugMode, $"CancelCellEdit : {uneditedValue}");
            base.CancelCellEdit(editingElement, uneditedValue);
        }

        /// <summary>
        /// Commits the cell edit.
        /// </summary>
        /// <param name="editingElement">The editing element.</param>
        /// <returns>True if the edit was committed successfully, otherwise false.</returns>
        protected override bool CommitCellEdit(FrameworkElement editingElement)
        {
            Debug.WriteLineIf(DebugMode, "CommitCellEdit");
            if (editingElement is TextBox tb)
            {
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = originalValue;
                }
            }

            return base.CommitCellEdit(editingElement);
        }

        /// <summary>
        /// Prepares the cell for editing.
        /// </summary>
        /// <param name="editingElement">The editing element.</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>The original value of the cell.</returns>
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "PrepareCellForEdit");

            try
            {
                // Determine the column type if not already determined
                if (fieldType == null)
                {
                    var filterDataGrid = (FilterDataGrid)DataGridOwner;
                    var dataContext = editingElement.DataContext;
                    culture = filterDataGrid.Translate.Culture;
                    var propertyName = ((Binding)Binding).Path.Path;
                    stringFormat = string.IsNullOrEmpty(((Binding)Binding).StringFormat)
                        ? string.Empty
                        : ((Binding)Binding).StringFormat.ToLower();

                    var fieldProperty = dataContext.GetType().GetProperty(propertyName);
                    if (fieldProperty != null)
                    {
                        fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                        BuildRegex();
                    }
                    else
                    {
                        Debug.WriteLineIf(DebugMode, "fieldProperty is null");
                    }
                }

                // Subscribe to keyboard and paste events
                if (editingElement is TextBox edit)
                {
                    originalValue = edit.Text;
                    edit.PreviewTextInput += OnPreviewTextInput;
                    DataObject.AddPastingHandler(edit, OnPaste);

                    // Create a new binding with the desired StringFormat and culture
                    var newBinding = new Binding(((Binding)Binding).Path.Path)
                    {
                        // removes formatting(symbol) for cell editing(TextBox)
                        // original formatting remains active for display(TextBlock)
                        StringFormat = string.Empty,
                        ConverterCulture = culture
                    };

                    edit.SetBinding(TextBox.TextProperty, newBinding);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugMode, $"Exception in PrepareCellForEdit: {ex.Message}");
            }

            return base.PrepareCellForEdit(editingElement, e);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Handles the paste event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnPaste");

            if (e.SourceDataObject.GetData(DataFormats.Text) is string pasteText && sender is TextBox textBox)
            {
                var newText = textBox.Text.Insert(textBox.SelectionStart, pasteText);

                if (!regex.IsMatch(newText))
                {
                    e.CancelCommand();
                }
            }
        }

        /// <summary>
        /// Handles the PreviewTextInput event of the TextBox control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnPreviewTextInput");

            if (sender is TextBox textBox)
            {
                var newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                var isNumeric = regex.IsMatch(newText);

                Debug.WriteLineIf(DebugMode, $"originalValue : {originalValue,-15}" +
                                             $"originalText : {textBox.Text,-15}" +
                                             $"newText : {newText,-15}" +
                                             $"IsTextNumeric : {isNumeric}");

                e.Handled = !isNumeric;
            }
        }

        #endregion Private Methods
    }

    public class DataGridTemplateColumn : System.Windows.Controls.DataGridTemplateColumn, IDataGridColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridTemplateColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(DataGridTemplateColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties
    }

    public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn, IDataGridColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridTextColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(DataGridTextColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties
    }
}