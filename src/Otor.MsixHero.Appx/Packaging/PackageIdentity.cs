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
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging
{
    public class PackageIdentity
    {
        public string AppName { get; set; }

        public Version AppVersion { get; set; }

        public AppxPackageArchitecture Architecture { get; set; }

        public string PublisherHash { get; set; }

        public string ResourceId { get; set; }

        public static PackageIdentity FromFullName(string packageFullName)
        {
            if (packageFullName == null)
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            var split = packageFullName.Split('_');
            if (split.Length < 5)
            {
                throw new ArgumentException("The full name must have 5 units.", nameof(packageFullName));
            }

            var obj = new PackageIdentity
            {
                AppName = split[0],
                AppVersion = Version.Parse(split[1]),
                Architecture = (AppxPackageArchitecture) Enum.Parse(typeof(AppxPackageArchitecture), split[2], true),
                ResourceId = split[3],
                PublisherHash = split[4]
            };

            return obj;
        }

        public override string ToString()
        {
            return $"{this.AppName}_{this.AppVersion}_{this.Architecture:G}_{this.ResourceId}_{this.PublisherHash}";
        }
    }
}
