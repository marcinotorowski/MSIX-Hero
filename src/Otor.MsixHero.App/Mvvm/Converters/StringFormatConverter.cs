using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Mvvm.Converters
{
    internal class StringFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            switch (values.Length)
            {
                case 0:
                {
                    return Binding.DoNothing;
                }

                case 1:
                {
                    return values[0];
                }

                case 2:
                {
                    return string.Format(ObjectToString(values[0]), values[1]);
                }

                case 3:
                {
                    return string.Format(ObjectToString(values[0]), values[1], values[2]);
                }

                case 4:
                {
                    return string.Format(ObjectToString(values[0]), values[1], values[2], values[3]);
                }

                default:
                {
                    return string.Format(ObjectToString(values[0]), values.Skip(1).ToArray());
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static string ObjectToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            if (obj == DependencyProperty.UnsetValue)
            {
                return string.Empty;
            }

            if (obj is string objString)
            {
                return objString;
            }

            return obj.ToString() ?? string.Empty;
        }
    }
}
