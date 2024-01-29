// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Mvvm.Converters
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
                case HumanizedDateHelper.HumanizedDate.Yesterday:
                case HumanizedDateHelper.HumanizedDate.ThisWeek:
                case HumanizedDateHelper.HumanizedDate.LastWeek:
                case HumanizedDateHelper.HumanizedDate.ThisMonth:
                case HumanizedDateHelper.HumanizedDate.LastMonth:
                case HumanizedDateHelper.HumanizedDate.Last6Months:
                case HumanizedDateHelper.HumanizedDate.ThisYear:
                case HumanizedDateHelper.HumanizedDate.LastYear:
                case HumanizedDateHelper.HumanizedDate.Older:
                    return MsixHeroTranslation.Instance["HumanDate_" + humanizedDate.ToString("G")];
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