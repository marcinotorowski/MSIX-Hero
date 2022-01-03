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

using System.Windows;

namespace Otor.MsixHero.App.Helpers.Validation
{
    public class ValidatedTabItem : DependencyObject
    {
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(ValidatedTabItem), new PropertyMetadata(true));
        public static readonly DependencyProperty ValidationMessageProperty = DependencyProperty.RegisterAttached("ValidationMessage", typeof(string), typeof(ValidatedTabItem), new PropertyMetadata(null));

        public static bool GetIsValid(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsValidProperty);
        }

        public static void SetIsValid(DependencyObject obj, bool value)
        {
            obj.SetValue(IsValidProperty, value);
        }

        public static string GetValidationMessage(DependencyObject obj)
        {
            return (string)obj.GetValue(ValidationMessageProperty);
        }

        public static void SetValidationMessage(DependencyObject obj, string value)
        {
            obj.SetValue(ValidationMessageProperty, value);
        }
    }
}
