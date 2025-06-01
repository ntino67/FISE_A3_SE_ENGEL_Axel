using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Converter
{
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 || 
                !(values[0] is double value) || 
                !(values[1] is double width) || 
                !(values[2] is double min) || 
                !(values[3] is double max))
            {
                return 0d;
            }

            if (max - min == 0)
                return 0d;

            double percent = (value - min) / (max - min);
            // S'assurer que percent est entre 0 et 1
            percent = Math.Max(0, Math.Min(percent, 1));
            
            return width * percent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}