using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;

namespace Otor.MsixHero.App.Converters
{
    public class GroupDateConverter : IValueConverter
    {
        private GroupDateConverter()
        {
        }

        public static GroupDateConverter Instance = new GroupDateConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            if (!(value is DateTime date))
            {
                return value;
            }

            return HumanizedDateHelper.GetHumanizedDate(date);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}