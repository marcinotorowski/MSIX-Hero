using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MSI_Hero.Converters
{
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return Binding.DoNothing;
        }
    }
}
