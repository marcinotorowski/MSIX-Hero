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
using System.Threading.Tasks;
using System.Threading;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging
{
    [Serializable]
    public class PackageEntry
    {
        public string PackageFullName { get; set; }

        public string PackageFamilyName { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Publisher { get; set; }

        public string DisplayPublisherName { get; set; }

        public Version Version { get; set; }

        public string ResourceId { get; set; }

        public AppxPackageArchitecture Architecture { get; set; }

        public MsixPackageType PackageType { get; set; }

        public string Description { get; set; }

        public DateTime? InstallDate { get; set; }

        public string InstallDirPath { get; set; }

        public string ManifestPath { get; set; }

        public string PsfConfigPath
        {
            get
            {
                if (InstallDirPath != null)
                {
                    return Path.Combine(InstallDirPath, "config.json");
                }

                if (ManifestPath != null)
                {
                    var dir = Path.GetDirectoryName(ManifestPath);
                    if (dir != null)
                    {
                        return Path.Combine(dir, "config.json");
                    }
                }

                return null;
            }
        }

        public string UserDirPath
        {
            get
            {
                return InstallDirPath == null
                    ? null
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", PackageFamilyName);
            }
        }

        public SignatureKind SignatureKind { get; set; }

        public string ImagePath { get; set; }

        public byte[] ImageContent { get; set; }

        public string TileColor { get; set; }

        public bool IsProvisioned { get; set; }

        public bool IsOptional { get; set; }

        public bool IsFramework { get; set; }

        public bool IsRunning { get; set; }

        public Uri AppInstallerUri { get; set; }

        public async Task<AppxPackage> ToAppxPackage(CancellationToken cancellationToken = default)
        {
            if (this.InstallDirPath == null && this.ManifestPath == null)
            {
                return null;
            }

            var manifestReader = new AppxManifestReader();

            using IAppxFileReader reader = this.ManifestPath != null && File.Exists(this.ManifestPath)
                ? new FileInfoFileReaderAdapter(this.ManifestPath)
                : new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, this.PackageFullName);

            return await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
        }
    }
}
