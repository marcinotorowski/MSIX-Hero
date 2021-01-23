// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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