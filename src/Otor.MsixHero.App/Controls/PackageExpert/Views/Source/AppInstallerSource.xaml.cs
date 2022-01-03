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
using System.Windows;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views.Source
{
    /// <summary>
    /// Interaction logic for AppInstallerSource.
    /// </summary>
    public partial class AppInstallerSource 
    {
        public static readonly DependencyProperty AppInstallerUriProperty = DependencyProperty.Register("AppInstallerUri", typeof(Uri), typeof(AppInstallerSource), new PropertyMetadata(null));

        public AppInstallerSource()
        {
            InitializeComponent();
        }

        public Uri AppInstallerUri
        {
            get => (Uri)GetValue(AppInstallerUriProperty);
            set => SetValue(AppInstallerUriProperty, value);
        }
    }
}
