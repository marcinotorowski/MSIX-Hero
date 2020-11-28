using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;

namespace Otor.MsixHero.App.Modules.EventViewer.Converters
{
    internal class EventLogIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogViewModel logViewModel)
            {
                switch (logViewModel.Level)
                {
                    case "Warning":
                        return this.IconWarning;
                    case "Error":
                        return this.IconError;
                }
            }

            return this.IconInformation;
        }

        public Geometry IconError { get; set; }

        public Geometry IconWarning { get; set; }

        public Geometry IconInformation { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
