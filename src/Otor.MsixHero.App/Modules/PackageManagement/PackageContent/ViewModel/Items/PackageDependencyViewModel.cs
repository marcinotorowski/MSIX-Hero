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

using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items
{
    public class PackageDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxPackageDependency model;

        public PackageDependencyViewModel(AppxPackageDependency model)
        {
            this.model = model;
        }

        public string Version => this.model.Dependency?.Version ?? this.model.Version;

        public string DisplayName => this.model.Dependency?.DisplayName ?? this.model.Name;

        public string DisplayPublisherName => this.model.Dependency?.PublisherDisplayName ?? this.model.Publisher;

        public string Name => this.model.Name;

        public string Publisher => this.model.Publisher;
    }
}