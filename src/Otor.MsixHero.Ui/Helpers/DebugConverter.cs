using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Otor.MsixHero.Ui.Helpers
{
    public class DebugConverter : MarkupExtension, IValueConverter
    {
        private static readonly DebugConverter Converter = new DebugConverter();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
