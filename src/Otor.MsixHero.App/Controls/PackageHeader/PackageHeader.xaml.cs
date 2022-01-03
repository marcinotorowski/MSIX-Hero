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
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls.PackageHeader
{
    /// <summary>
    /// Interaction logic for PackageHeader.
    /// </summary>
    public partial class PackageHeader
    {
        public static readonly DependencyProperty LogoProperty = DependencyProperty.Register("Logo", typeof(ImageSource), typeof(PackageHeader), new PropertyMetadata(null));

        public static readonly DependencyProperty PublisherProperty = DependencyProperty.Register("Publisher", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register("Version", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));

        public static readonly DependencyProperty PackageNameProperty = DependencyProperty.Register("PackageName", typeof(string), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty TileColorProperty = DependencyProperty.Register("TileColor", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(Brushes.DarkGray));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(null));
        
        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(PackageHeader), new PropertyMetadata(Brushes.White));


        public PackageHeader()
        {
            InitializeComponent();
        }

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public ImageSource Logo
        {
            get => (ImageSource)GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public Brush TileColor
        {
            get => (Brush)GetValue(TileColorProperty);
            set => SetValue(TileColorProperty, value);
        }

        public string Publisher
        {
            get => (string)GetValue(PublisherProperty);
            set => SetValue(PublisherProperty, value);
        }

        public string PackageName
        {
            get => (string)GetValue(PackageNameProperty);
            set => SetValue(PackageNameProperty, value);
        }


    }
}
