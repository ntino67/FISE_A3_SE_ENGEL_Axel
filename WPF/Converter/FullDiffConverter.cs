using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.Converter
{
    public class FullDiffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = value is bool b && b;
            return isActive ? "Full" : "Differential";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "Full";
        }
    }
}