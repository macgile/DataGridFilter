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

namespace FilterDataGrid.Converters
{
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        #region Private Constants

        private const string CultureParameter = "Culture";

        #endregion Private Constants

        #region ValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion ValueConverter

        #region MultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // values [0] contains the format
                if (values[0] == DependencyProperty.UnsetValue || string.IsNullOrEmpty(values[0]?.ToString()))
                    return string.Empty;

                string stringFormat = values[0].ToString() ?? string.Empty;

                // the last item of values array is culture
                if (parameter != null && parameter.Equals(CultureParameter))
                {
                    object lastValue = values.LastOrDefault();
                    culture = lastValue != null ? (CultureInfo)lastValue : culture;
                }

                object[] formatArgs = [.. values.Skip(1)];
                return string.Format(culture, stringFormat, formatArgs);
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

        #endregion MultiValueConverter
    }
}