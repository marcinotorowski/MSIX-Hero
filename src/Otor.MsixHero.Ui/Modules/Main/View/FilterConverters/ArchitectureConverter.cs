using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Ui.Modules.Main.View.FilterConverters
{
    public class ArchitectureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            var pf = (PackageFilter)value & PackageFilter.AllArchitectures;
            if (pf == PackageFilter.AllArchitectures || pf == 0)
            {
                return "All architectures";
            }

            var values = Enum.GetValues(typeof(PackageFilter)).OfType<PackageFilter>().Where(f => (f & pf) == f).ToArray();
            switch (values.Length)
            {
                case 1:
                    switch (pf)
                    {
                        case PackageFilter.x64:
                            return "Only 64-bit";
                        case PackageFilter.x86:
                            return "Only 32-bit";
                        case PackageFilter.Neutral:
                            return "Only neutral";
                        case PackageFilter.Arm:
                            return "Only ARM";
                        case PackageFilter.Arm64:
                            return "Only ARM64";
                        default:
                            return Binding.DoNothing;
                    }
                default:
                    return string.Join(" + ", values.Select(Translate).Where(v => v != null));
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
                case PackageFilter.x64:
                    return "64-bit";
                case PackageFilter.x86:
                    return "32-bit";
                case PackageFilter.Neutral:
                    return "Neutral";
                case PackageFilter.Arm:
                    return "ARM";
                case PackageFilter.Arm64:
                    return "ARM64";
                default:
                    return null;
            }
        }
    }
}
