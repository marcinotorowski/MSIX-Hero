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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    public static class InstalledPackageExtensions
    {
        public static async Task<AppxPackage> ToAppxPackage(this InstalledPackage pkg, CancellationToken cancellationToken = default)
        {
            if (pkg.InstallLocation == null)
            {
                return null;
            }

            var manifestReader = new AppxManifestReader();

            IAppxFileReader reader;
            if (pkg.ManifestLocation == null || !File.Exists(pkg.ManifestLocation))
            {
                reader = new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, pkg.PackageId);
            }
            else
            {
                reader = new FileInfoFileReaderAdapter(pkg.ManifestLocation);
            }

            using (reader)
            {
                return await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
