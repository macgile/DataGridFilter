#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : WpfCodeProject
// Projet     : WpfCodeProject
// File       : StringFormatConverter.cs
// Created    : 26/01/2021
//

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

// ReSharper disable CheckNamespace

namespace FilterDataGrid
{
    public class StringFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // values [0] contains the format
                if (values[0] == DependencyProperty.UnsetValue || string.IsNullOrEmpty(values[0]?.ToString()))
                    return string.Empty;

                var stringFormat = values[0].ToString();

                return string.Format(stringFormat, values.ToArray().Skip(1).ToArray());
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"StringFormatConverter.Convert error: {ex.Message}");
                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}