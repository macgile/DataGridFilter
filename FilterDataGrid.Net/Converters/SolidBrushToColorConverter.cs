#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : FilterDataGrid.Net5.0
// File       : SolidBrushToColorConverter.cs
// Created    : 02/06/2022
//

#endregion

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FilterDataGrid.Converters
{
    [ValueConversion(typeof(SolidColorBrush), typeof(Color))]
    public class SolidBrushToColorConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SolidColorBrush result) return null;
            return result.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}