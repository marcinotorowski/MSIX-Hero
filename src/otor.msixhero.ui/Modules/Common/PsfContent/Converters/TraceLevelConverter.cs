using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.ui.Modules.Common.PsfContent.Converters
{
    public class TraceLevelConverter : MarkupExtension, IValueConverter
    {
        // ReSharper disable once InconsistentNaming
        private static TraceLevelConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            switch ((TraceLevel) value)
            {
                case TraceLevel.UnexpectedFailures:
                    return "Unexpected failures";
                case TraceLevel.Always:
                    return "Unexpected failures";
                case TraceLevel.IgnoreSuccess:
                    return "Ignore success";
                case TraceLevel.AllFailures:
                    return "All failures";
                case TraceLevel.Ignore:
                    return "Ignore";
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var me = instance ?? (instance = new TraceLevelConverter());
            return me;
        }
    }
}
