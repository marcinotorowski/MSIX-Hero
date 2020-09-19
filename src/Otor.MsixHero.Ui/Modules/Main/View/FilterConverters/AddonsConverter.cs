using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Ui.Modules.Main.View.FilterConverters
{
    public class AddonsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            var pf = (PackageFilter)value & PackageFilter.MainAppsAndAddOns;

            switch (pf)
            {
                case PackageFilter.MainApps:
                    return "Only main apps";
                case PackageFilter.Addons:
                    return "Only add-on apps";
                default:
                    return "Main apps + add-ons";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
