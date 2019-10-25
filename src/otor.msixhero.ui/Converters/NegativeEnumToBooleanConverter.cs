using System;
using System.Globalization;
using System.Windows.Data;

namespace otor.msixhero.ui.Converters
{
    public class NegativeEnumToBooleanConverter : IValueConverter
    {
        // Convert enum [value] to boolean, true if matches [param]
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            return !value.Equals(param);
        }

        // Convert boolean to enum, returning [param] if true
        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            return (bool)value ? Binding.DoNothing : param;
        }
    }
}