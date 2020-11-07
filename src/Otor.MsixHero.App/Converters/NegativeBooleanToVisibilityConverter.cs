using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters
{
    public class NegativeBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            if (value is Visibility visibilityValue)
            {
                return visibilityValue == Visibility.Visible;
            }

            return Binding.DoNothing;
        }
    }
}
