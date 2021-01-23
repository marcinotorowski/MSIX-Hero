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

using Otor.MsixHero.Dependencies.Domain;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Model
{
    public class PackageDependencyVertex : DependencyVertex
    {
        private readonly InstalledPackageGraphElement packageGraphDependency;

        public PackageDependencyVertex(InstalledPackageGraphElement packageGraphDependency)
        {
            this.packageGraphDependency = packageGraphDependency;
        }

        public string Logo => this.packageGraphDependency.Package.Image;

        public string TileColor => this.packageGraphDependency.Package.TileColor;

        public string DisplayName => this.packageGraphDependency.Package.DisplayName;

        public string Architecture => this.packageGraphDependency.Package.Architecture;

        public string Version => this.packageGraphDependency.Package.Version.ToString();

        public string PublisherDisplayName => this.packageGraphDependency.Package.DisplayPublisherName;
    }
}