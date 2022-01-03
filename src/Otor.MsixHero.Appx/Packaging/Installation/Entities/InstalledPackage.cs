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
using System.IO;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Appx.Packaging.Installation.Entities
{
    [Serializable]
    public class InstalledPackage
    {
        public PackageContext Context { get; set; }

        public string PackageId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string DisplayPublisherName { get; set; }

        public string Name { get; set; }

        public string Publisher { get; set; }

        public DateTime InstallDate { get; set; }

        public Version Version { get; set; }

        public string Architecture { get; set; }

        public MsixPackageType PackageType { get; set; }

        public string InstallLocation { get; set; }

        public string ManifestLocation
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(this.InstallLocation, FileConstants.AppxManifestFile);
            }
        }

        public string PsfConfig
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(this.InstallLocation, "config.json");
            }
        }

        public string PackageFamilyName { get; set; }

        public SignatureKind SignatureKind { get; set; }

        public string Image { get; set; }

        public string TileColor { get; set; }

        public RegistryMountState HasRegistry { get; set; }

        public string UserDataPath
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.PackageFamilyName);
            }
        }

        public bool IsProvisioned { get; set; }

        public bool IsOptional { get; set; }

        public bool IsFramework { get; set; }

        public Uri AppInstallerUri { get; set; }

        public bool IsRunning { get; set; }
    }
}
