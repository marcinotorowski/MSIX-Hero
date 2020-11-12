using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.App.Converters
{
    public class MsixPackageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is PackageTypeDisplay type))
            {
                type = PackageTypeDisplay.Normal;
            }

            if (!(value is MsixPackageType msixPackageType))
            {
                return Binding.DoNothing;
            }

            return PackageTypeConverter.GetPackageTypeStringFrom(msixPackageType, type).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
