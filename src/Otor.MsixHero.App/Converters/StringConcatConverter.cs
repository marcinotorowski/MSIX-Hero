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
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Otor.MsixHero.App.Converters;

internal class StringConcatConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        switch (values.Length)
        {
            case 0:
            {
                return Binding.DoNothing;
            }

            case 1:
            {
                return values[0];
            }

            case 2:
            {
                var splitWithSpaces = parameter is bool boolParam && boolParam;

                if (splitWithSpaces)
                {
                    return ObjectToString(values[0]) + " " + ObjectToString(values[1]);
                }

                return ObjectToString(values[0]) + ObjectToString(values[1]);
            }

            case 3:
            {
                var splitWithSpaces = parameter is bool boolParam && boolParam;

                if (splitWithSpaces)
                {
                    return ObjectToString(values[0]) + " " + ObjectToString(values[1]) + " " + ObjectToString(values[2]);
                }

                return ObjectToString(values[0]) + ObjectToString(values[1]) + ObjectToString(values[2]);
            }

            case 4:
            {
                var splitWithSpaces = parameter is bool boolParam && boolParam;

                if (splitWithSpaces)
                {
                    return ObjectToString(values[0]) + " " + ObjectToString(values[1]) + " " + ObjectToString(values[2]) + " " + ObjectToString(values[3]);
                }

                return ObjectToString(values[0]) + ObjectToString(values[1]) + ObjectToString(values[2]) + ObjectToString(values[3]);
            }

            default:
            {
                var splitWithSpaces = parameter is bool boolParam && boolParam;
                var returned = new StringBuilder();

                for (var i = 0; i < values.Length; i++)
                {
                    if (splitWithSpaces && i > 0)
                    {
                        returned.Append(' ');
                    }

                    returned.Append(ObjectToString(values[i]));
                }

                return returned.ToString();
            }
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static string ObjectToString(object obj)
    {
        if (obj == null)
        {
            return null;
        }

        if (obj == DependencyProperty.UnsetValue)
        {
            return null;
        }

        if (obj is string objString)
        {
            return objString;
        }

        return obj.ToString();
    }
}