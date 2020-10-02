using System;
using System.Globalization;
using System.Windows.Data;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Ui.Modules.PackageList.View
{
    internal class PackageContextDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PackageContext context)
            {
                switch (context)
                {
                    case PackageContext.CurrentUser:
                        return "Current user";
                    case PackageContext.AllUsers:
                        return "All users";
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}