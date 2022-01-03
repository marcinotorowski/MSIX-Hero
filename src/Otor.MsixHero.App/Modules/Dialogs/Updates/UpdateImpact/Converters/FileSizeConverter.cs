// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Windows.Data;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.Converters
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long longValue)
            {
                return Convert(longValue);
            }
            
            if (value is int intValue)
            {
                return Convert(intValue);
            }

            if (value != null && long.TryParse(value.ToString(), out var parsed))
            {
                return Convert(parsed);
            }

            return "?";
        }

        private static string Convert(long value)
        {
            var units = new[] {"B", "KB", "MB", "GB", "TB"};

            var hasMinus = value < 0;

            var doubleValue = (double) Math.Abs(value);
            var index = 0;
            while (doubleValue > 1024 && index < units.Length)
            {
                doubleValue /= 1024.0;
                index++;
            }

            if (index == 0)
            {
                return value + " " + units[index];
            }

            return (hasMinus ? "-" : string.Empty) + (Math.Round(doubleValue * 10, 0) / 10.0).ToString("0.0") + " " + units[index];
        }

        private static string Convert(int value)
        {
            return Convert((long) value);
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
