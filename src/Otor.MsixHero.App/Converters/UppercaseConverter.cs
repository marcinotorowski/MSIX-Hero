using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters
{
    public class UppercaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            if (value is string valString)
            {
                return valString.ToUpper();
            }

            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            return value.ToString()?.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}