#region (c) 2019 Gilles Macabies

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : DataGridTextColumn.cs
// Created    : 09/11/2019

#endregion (c) 2019 Gilles Macabies

using System.Windows;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

namespace FilterDataGrid
{
    public sealed class DataGridTemplateColumn : System.Windows.Controls.DataGridTemplateColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(DataGridTemplateColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register("IsColumnFiltered", typeof(bool), typeof(DataGridTemplateColumn),
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

    public sealed class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(DataGridTextColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register("IsColumnFiltered", typeof(bool), typeof(DataGridTextColumn),
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