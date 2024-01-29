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

using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Visuals
{
    public class InstalledDependencyVertex : DependencyVertex
    {
        private readonly AppxPackage packageDependency;

        public InstalledDependencyVertex(AppxPackage packageDependency)
        {
            this.packageDependency = packageDependency;
        }

        public byte[] Logo => this.packageDependency.Logo;

        public string TileColor => "#dddddd";

        public string DisplayName => this.packageDependency.DisplayName;
        
        public string FullName => this.packageDependency.FullName;

        public string Architecture => this.packageDependency.ProcessorArchitecture.ToString();

        public string Version => this.packageDependency.Version;

        public string PublisherDisplayName => this.packageDependency.PublisherDisplayName;
    }
}