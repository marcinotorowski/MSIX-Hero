using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Ui.Modules.Main.View.FilterConverters
{
    public class RunningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            var pf = (PackageFilter) value & PackageFilter.InstalledAndRunning;

            switch (pf)
            {
                case PackageFilter.Running:
                    return "Only running apps";
                case PackageFilter.Installed:
                default:
                    return "Installed apps";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
