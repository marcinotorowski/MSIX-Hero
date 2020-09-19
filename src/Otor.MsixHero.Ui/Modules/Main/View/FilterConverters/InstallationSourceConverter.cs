using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Ui.Modules.Main.View.FilterConverters
{
    public class InstallationSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            var pf = (PackageFilter) value & PackageFilter.AllSources;
            if (pf == PackageFilter.AllSources || pf == 0)
            {
                return "All installation sources";
            }

            var values = new[] { PackageFilter.Developer, PackageFilter.Store, PackageFilter.System }.Where(f => (f & pf) == f).ToArray();
            switch (values.Length)
            {
                case 1:
                    switch (pf)
                    {
                        case PackageFilter.Store:
                            return "Only Store apps";
                        case PackageFilter.System:
                            return "Only system apps";
                        case PackageFilter.Developer:
                            return "Only side-loaded apps";
                        default:
                            return Binding.DoNothing;
                    }
                default:
                    return string.Join(" + ", values.Select(Translate).Where(v => v != null)) + " apps";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private static string Translate(PackageFilter value)
        {
            switch (value)
            {
                case PackageFilter.Store:
                    return "Store";
                case PackageFilter.System:
                    return "System";
                case PackageFilter.Developer:
                    return "Side-loaded";
                default:
                    return null;
            }
        }
    }
}
