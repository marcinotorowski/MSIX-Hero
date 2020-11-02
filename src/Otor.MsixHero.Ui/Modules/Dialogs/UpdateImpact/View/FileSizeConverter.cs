using System;
using System.Globalization;
using System.Windows.Data;

namespace Otor.MsixHero.Ui.Modules.Dialogs.UpdateImpact.View
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long longValue)
            {
                return Convert(longValue);
            }
            
            if (value is int intValue)
            {
                return Convert(intValue);
            }

            if (value != null && long.TryParse(value.ToString(), out var parsed))
            {
                return Convert(parsed);
            }

            return "?";
        }

        private static string Convert(long value)
        {
            var units = new[] {"B", "KB", "MB", "GB", "TB"};

            var hasMinus = value < 0;

            var doubleValue = (double) Math.Abs(value);
            var index = 0;
            while (doubleValue > 1024 && index < units.Length)
            {
                doubleValue /= 1024.0;
                index++;
            }

            if (index == 0)
            {
                return value + " " + units[index];
            }

            return (hasMinus ? "-" : string.Empty) + (Math.Round(doubleValue * 10, 0) / 10.0).ToString("0.0") + " " + units[index];
        }

        private static string Convert(int value)
        {
            return Convert((long) value);
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
