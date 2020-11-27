using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Modules.PackageManagement.Views.Converters
{
    public class FilterHeaderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            if (value is string stringValue)
            {
                if (stringValue.IndexOf("all", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
