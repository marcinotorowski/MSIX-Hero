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

using System.Diagnostics;
using System.Windows;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.View.Installation.Source
{
    public partial class StoreSourceView
    {
        public static readonly DependencyProperty FamilyNameProperty = DependencyProperty.Register("FamilyName", typeof(string), typeof(StoreSourceView), new PropertyMetadata(null));

        public StoreSourceView()
        {
            InitializeComponent();
        }

        public string FamilyName
        {
            get => (string)GetValue(FamilyNameProperty);
            set => SetValue(FamilyNameProperty, value);
        }

        private void OpenStorePage(object sender, RoutedEventArgs e)
        {
            var link = $"ms-windows-store://pdp/?PFN={this.FamilyName}";
            var psi = new ProcessStartInfo(link);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void WriteReview(object sender, RoutedEventArgs e)
        {
            var link = $"ms-windows-store://review/?PFN={this.FamilyName}";
            var psi = new ProcessStartInfo(link);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}
