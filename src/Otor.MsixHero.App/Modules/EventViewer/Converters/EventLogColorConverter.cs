using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;

namespace Otor.MsixHero.App.Modules.EventViewer.Converters
{
    internal class EventLogColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogViewModel logViewModel)
            {
                switch (logViewModel.Level)
                {
                    case "Warning":
                        return this.ColorWarning;
                    case "Error":
                        return this.ColorError;
                }
            }

            return this.ColorInformation;
        }

        public Color ColorError { get; set; }

        public Color ColorWarning { get; set; }

        public Color ColorInformation { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}