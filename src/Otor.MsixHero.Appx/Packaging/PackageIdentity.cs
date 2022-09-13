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
using System.Text.RegularExpressions;
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

        public string GetFamilyName() => this.AppName + "_" + this.PublisherHash;

        public static PackageIdentity FromFullName(string packageFullName)
        {
            if (packageFullName == null)
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            var split = packageFullName.Split('_');
            if (split.Length < 5)
            {
                throw new ArgumentException(Resources.Localization.Packages_Error_FullName, nameof(packageFullName));
            }

            if (!Version.TryParse(split[1], out var parsedVersion))
            {
                throw new ArgumentException(string.Format("The value '{0}' is not a valid full package name. String '{1}' is not a valid version.", packageFullName, split[1]), nameof(packageFullName));
            }

            if (!Enum.TryParse(split[2], true, out AppxPackageArchitecture parsedArchitecture))
            {
                throw new ArgumentException(string.Format("The value '{0}' is not a valid full package name. String '{1}' is not a valid package architecture.", packageFullName, split[2]), nameof(packageFullName));
            }

            if (string.IsNullOrEmpty(split[4]) || !Regex.IsMatch(split[4], @"^[0123456789abcdefghjkmnpqrstvwxyz]{13}$", RegexOptions.IgnoreCase))
            {
                throw new ArgumentException(string.Format("The value '{0}' is not a valid full package name. String '{1}' is not a valid publisher hash.", packageFullName, split[4]), nameof(packageFullName));
            }

            var obj = new PackageIdentity
            {
                AppName = split[0],
                Architecture = parsedArchitecture,
                ResourceId = split[3],
                PublisherHash = split[4],
                AppVersion = parsedVersion
            };
            
            return obj;
        }

        public static bool TryFromFullName(string packageFullName, out PackageIdentity identity)
        {
            if (packageFullName == null)
            {
                identity = default;
                return false;
            }

            var split = packageFullName.Split('_');
            if (split.Length < 5)
            {
                identity = default;
                return false;
            }

            identity = new PackageIdentity
            {
                AppName = split[0],
                ResourceId = split[3],
                PublisherHash = split[4]
            };

            if (!Version.TryParse(split[1], out var parsedVersion))
            {
                return false;
            }

            if (!Enum.TryParse(split[2], true, out AppxPackageArchitecture parsedArchitecture))
            {
                return false;
            }

            identity.AppVersion = parsedVersion;
            identity.Architecture = parsedArchitecture;

            return true;


        }

        public override string ToString()
        {
            return $"{this.AppName}_{this.AppVersion}_{this.Architecture:G}_{this.ResourceId}_{this.PublisherHash}";
        }
    }
}
