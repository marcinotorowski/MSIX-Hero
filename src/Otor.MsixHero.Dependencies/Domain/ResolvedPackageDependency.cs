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

using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.Dependencies.Domain
{
    public class ResolvedPackageDependency : PackageDependency, IResolvedPackageDependency
    {
        public ResolvedPackageDependency(AppxPackage package, Version minVersion) : base(package.Name, package.Publisher, minVersion)
        {
            this.InstalledVersion = System.Version.Parse(package.Version);
            this.DisplayName = package.DisplayName;
            this.PublisherDisplayName = package.PublisherDisplayName;
            this.Package = package;
        }

        public AppxPackage Package { get; }

        public string DisplayName { get; }

        public string PublisherDisplayName { get; }

        public Version InstalledVersion { get; }
    }
}