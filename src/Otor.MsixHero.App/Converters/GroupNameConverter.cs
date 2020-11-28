using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;

namespace Otor.MsixHero.App.Converters
{
    public class GroupNameConverter : IValueConverter
    {
        public static GroupNameConverter Instance = new GroupNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return Binding.DoNothing;
            }

            if (!(value is HumanizedDateHelper.HumanizedDate humanizedDate))
            {
                if (!(value is DateTime date))
                {
                    return value;
                }

                humanizedDate = HumanizedDateHelper.GetHumanizedDate(date);
            }

            switch (humanizedDate)
            {
                case HumanizedDateHelper.HumanizedDate.Today:
                    return HumanizedDateHelper.LabelToday;
                case HumanizedDateHelper.HumanizedDate.Yesterday:
                    return HumanizedDateHelper.LabelYesterday;
                case HumanizedDateHelper.HumanizedDate.ThisWeek:
                    return HumanizedDateHelper.LabelThisWeek;
                case HumanizedDateHelper.HumanizedDate.LastWeek:
                    return HumanizedDateHelper.LabelLastWeek;
                case HumanizedDateHelper.HumanizedDate.ThisMonth:
                    return HumanizedDateHelper.LabelThisMonth;
                case HumanizedDateHelper.HumanizedDate.LastMonth:
                    return HumanizedDateHelper.LabelLastMonth;
                case HumanizedDateHelper.HumanizedDate.Last6Months:
                    return HumanizedDateHelper.LabelLast6Months;
                case HumanizedDateHelper.HumanizedDate.ThisYear:
                    return HumanizedDateHelper.LabelThisYear;
                case HumanizedDateHelper.HumanizedDate.LastYear:
                    return HumanizedDateHelper.LabelLastYear;
                case HumanizedDateHelper.HumanizedDate.Older:
                    return HumanizedDateHelper.LabelOlder;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}