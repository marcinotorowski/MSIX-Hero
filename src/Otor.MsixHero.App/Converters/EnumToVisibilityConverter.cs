using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        // Convert enum [value] to boolean, true if matches [param]
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            return value.Equals(param) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Convert boolean to enum, returning [param] if true
        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}