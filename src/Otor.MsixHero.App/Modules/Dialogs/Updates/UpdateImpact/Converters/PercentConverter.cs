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
using System.Windows.Data;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.Converters
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
