using System;
using System.Globalization;
using System.Windows.Data;

namespace Otor.MsixHero.App.Controls.Volume.Converters
{
    internal enum VolumeSizeCaptionConverterMode
    {
        OccupiedTotal,
        OccupiedFree,
        FreeTotal
    }

    internal class VolumeSizeCaptionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return Binding.DoNothing;
            }

            if (!(values[0] is long val1))
            {
                return Binding.DoNothing;
            }

            if (!(values[1] is long val2))
            {
                return Binding.DoNothing;
            }

            long totalSize;
            long occupiedSize;

            if (!(parameter is VolumeSizeCaptionConverterMode mode))
            {
                mode = VolumeSizeCaptionConverterMode.OccupiedTotal;
            }

            switch (mode)
            {
                case VolumeSizeCaptionConverterMode.OccupiedTotal:
                    occupiedSize = val1;
                    totalSize = val2;
                    break;
                case VolumeSizeCaptionConverterMode.OccupiedFree:
                    occupiedSize = val1;
                    totalSize = val1 + val2;
                    break;
                case VolumeSizeCaptionConverterMode.FreeTotal:
                    occupiedSize = val2 - val1;
                    totalSize = val2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var sizeFree = FormatSize(totalSize - occupiedSize);
            var sizeTotal = FormatSize(totalSize);
            return $"{sizeFree} free of {sizeTotal}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var aa = new object[targetTypes.Length];
            Array.Fill(aa, Binding.DoNothing);
            return aa;
        }

        private static string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1000)
            {
                return sizeInBytes + " B";
            }

            var units = new[] { "TB", "GB", "MB", "KB" };

            double size = sizeInBytes;
            for (var i = units.Length - 1; i >= 0; i--)
            {
                size /= 1024.0;

                if (size < 1024)
                {
                    return $"{size:0} {units[i]}";
                }

                if (size < 10 * 1024 && i > 0)
                {
                    i--;
                    size = Math.Floor(100.0 * size / 1024) / 100;
                    return $"{size:0.00} {units[i]}";
                }
            }

            return $"{size:0} {units[0]}";
        }
    }
}
