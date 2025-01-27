#region (c) 2019 Gilles Macabies

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : DataGridTextColumn.cs
// Created    : 09/11/2019

#endregion (c) 2019 Gilles Macabies

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable InvertIf
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

namespace FilterDataGrid
{
    public sealed class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn
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

    public sealed class DataGridComboBoxColumn : System.Windows.Controls.DataGridComboBoxColumn
    {
        #region Public Properties

        public List<ItemsSourceMembers> ComboBoxItemsSource { get; set; }
        public bool IsSingle { get; set; }

        #endregion Public Properties

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

            // Determine the column type if not already determined
            if (fieldType == null)
            {
                var filterDataGrid = (FilterDataGrid)DataGridOwner;
                var dataContext = editingElement.DataContext;
                culture = filterDataGrid.Translate.Culture;
                var propertyName = ((Binding)Binding).Path.Path;
                stringFormat = string.IsNullOrEmpty(((Binding)Binding).StringFormat)
                    ? ""
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
                    StringFormat = "",
                    ConverterCulture = culture
                };

                edit.SetBinding(TextBox.TextProperty, newBinding);
            }

            return base.PrepareCellForEdit(editingElement, e);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Builds the regex for keyboard input verification.
        /// </summary>
        /// <param name="culture">The culture info.</param>
        private void BuildRegex()
        {
            Debug.WriteLineIf(DebugMode, $"BuildRegex : {fieldType}");
            var nfi = culture.NumberFormat;

            switch (fieldType)
            {
                case Type _ when fieldType == typeof(double):
                case Type _ when fieldType == typeof(decimal):
                case Type _ when fieldType == typeof(float):
                    var decimalSeparator = stringFormat.Contains("c")
                        ? Regex.Escape(nfi.CurrencyDecimalSeparator)
                        : Regex.Escape(nfi.NumberDecimalSeparator);
                    regex = new Regex($@"^-?(\d+({decimalSeparator}\d*)?|{decimalSeparator}\d*)?$");
                    break;

                case Type _ when fieldType == typeof(sbyte):
                case Type _ when fieldType == typeof(short):
                case Type _ when fieldType == typeof(int):
                case Type _ when fieldType == typeof(long):
                    regex = new Regex(@"^-?\d+$");
                    break;

                case Type _ when fieldType == typeof(byte):
                case Type _ when fieldType == typeof(ushort):
                case Type _ when fieldType == typeof(uint):
                case Type _ when fieldType == typeof(ulong):
                    regex = new Regex(@"^\d+$");
                    break;

                default:
                    Debug.WriteLineIf(DebugMode, "Unsupported fieldType");
                    regex = new Regex(@"[^\t\r\n]+");
                    break;
            }
        }

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

    public sealed class DataGridTemplateColumn : System.Windows.Controls.DataGridTemplateColumn
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

    public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
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

    public class ItemsSourceMembers
    {
        #region Public Properties

        public string DisplayMember { get; set; }
        public string SelectedValue { get; set; }

        #endregion Public Properties
    }
}