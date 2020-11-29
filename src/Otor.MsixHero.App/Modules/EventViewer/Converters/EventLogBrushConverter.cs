using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;

namespace Otor.MsixHero.App.Modules.EventViewer.Converters
{
    internal class EventLogBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogViewModel logViewModel)
            {
                switch (logViewModel.Level)
                {
                    case "Warning":
                        return this.BrushWarning;
                    case "Error":
                        return this.BrushError;
                    case "Verbose":
                        return this.BrushVerbose;
                }
            }

            return this.BrushInformation;
        }

        public Brush BrushError { get; set; }

        public Brush BrushWarning { get; set; }

        public Brush BrushInformation { get; set; }
        
        public Brush BrushVerbose { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}