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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Otor.MsixHero.App.Helpers
{
    public class ButtonClosePopup : Behavior<ButtonBase>
    {
        public static readonly DependencyProperty PopupOwnerProperty = DependencyProperty.RegisterAttached("PopupOwner", typeof(Popup), typeof(ButtonClosePopup), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseUp += ButtonMouseUp;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseUp -= ButtonMouseUp;
            base.OnDetaching();
        }
        
        public static Popup GetPopupOwner(DependencyObject obj)
        {
            return (Popup)obj.GetValue(PopupOwnerProperty);
        }

        public static void SetPopupOwner(DependencyObject obj, Popup value)
        {
            obj.SetValue(PopupOwnerProperty, value);
        }
        
        private static void ButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            var button = (ButtonBase) sender;
            var contentWithPopup = button.GetVisualParent<DependencyObject>(d => GetPopupOwner(d) != null);
            if (contentWithPopup != null)
            {
                GetPopupOwner(contentWithPopup).IsOpen = false;
            }
        }
    }
}
