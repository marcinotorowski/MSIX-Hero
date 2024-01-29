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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls.TreeListView
{
    public class TreeListViewConverter : IValueConverter
    {
        public const double Indentation = 16;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) 
            {
                return null;
            }
            
            if (targetType == typeof(double) && value is DependencyObject dependencyObject)
            {
                var level = -1;
             
                for (; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
                {
                    if (dependencyObject is TreeViewItem)
                    {
                        level++;
                    }
                }

                return Indentation * level;
            }

            throw new NotSupportedException($"Cannot convert from {value.GetType()} to {targetType}.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}