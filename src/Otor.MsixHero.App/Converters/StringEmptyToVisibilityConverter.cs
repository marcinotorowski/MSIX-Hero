using System;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters
{
    public class StringEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Visibility.Visible;
            }

            if (value is string s)
            {
                return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return string.IsNullOrEmpty(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
