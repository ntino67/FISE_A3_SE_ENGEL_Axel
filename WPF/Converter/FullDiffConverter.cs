using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.Converter
{
    public class FullDiffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Implémentation de base
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}