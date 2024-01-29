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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls
{
    public class LabelWithIcon : ContentControl
    {
        public static readonly DependencyProperty IsCollapsibleProperty = DependencyProperty.Register(nameof(IsCollapsible), typeof(bool), typeof(LabelWithIcon), new PropertyMetadata(false));

        public static readonly DependencyProperty Icon16x16Property = DependencyProperty.Register("Icon16x16", typeof(Geometry), typeof(LabelWithIcon), new PropertyMetadata(Geometry.Empty));

        static LabelWithIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelWithIcon), new FrameworkPropertyMetadata(typeof(LabelWithIcon)));
        }
        
        public Geometry Icon16x16   
        {
            get => (Geometry)GetValue(Icon16x16Property);
            set => SetValue(Icon16x16Property, value);
        }

        public bool IsCollapsible
        {
            get => (bool)GetValue(IsCollapsibleProperty);
            set => SetValue(IsCollapsibleProperty, value);
        }
    }
}
