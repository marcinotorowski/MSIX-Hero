using System;
using System.Globalization;
using System.Windows.Data;

namespace Otor.MsixHero.App.Modules.Editors.Updates.UpdateImpact.View
{
    public class PercentConverter : IMultiValueConverter
    {
        public int Round { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2)
            {
                return Binding.DoNothing;
            }

            if (values[0] is long firstLong && values[1] is long secondLong)
            {
                if (secondLong == 0)
                {
                    return "0%";
                }

                var ratio = Math.Round(100.0 * firstLong / secondLong, this.Round);
                if (this.Round == 0)
                {
                    return ratio.ToString("0") + "%";
                }

                return ratio.ToString("0." + new string('0', this.Round)) + "%";
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
