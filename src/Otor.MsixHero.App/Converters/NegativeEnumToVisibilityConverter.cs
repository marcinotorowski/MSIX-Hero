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
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters
{
    public class NegativeEnumToVisibilityConverter : IValueConverter
    {
        // Convert enum [value] to boolean, true if matches [param]
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            return !value.Equals(param) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Convert boolean to enum, returning [param] if true
        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}