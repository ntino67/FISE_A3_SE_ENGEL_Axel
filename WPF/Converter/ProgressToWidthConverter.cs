using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Converter
{
    public class ProgressToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress && progress >= 0)
            {
                // Si un paramètre est passé, utilisez-le comme largeur maximale
                double maxWidth = 100;

                if (parameter != null && double.TryParse(parameter.ToString(), out double width))
                {
                    maxWidth = width;
                }

                // Obtenir la largeur du conteneur parent si disponible
                var frameworkElement = parameter as FrameworkElement;
                if (frameworkElement != null)
                {
                    maxWidth = frameworkElement.ActualWidth;
                }

                return (progress / 100.0) * maxWidth;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}