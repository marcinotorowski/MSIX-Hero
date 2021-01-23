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

using System.Windows;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View
{
    public class SearchHelper : DependencyObject
    {
        public static readonly DependencyProperty SearchStringProperty = DependencyProperty.RegisterAttached("SearchString", typeof(string), typeof(SearchHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static string GetSearchString(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchStringProperty);
        }

        public static void SetSearchString(DependencyObject obj, string value)
        {
            obj.SetValue(SearchStringProperty, value);
        }
    }
}
