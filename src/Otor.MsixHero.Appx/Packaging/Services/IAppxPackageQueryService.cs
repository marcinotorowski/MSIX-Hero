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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.Services
{
    public interface IAppxPackageQueryService
    {
        Task<List<PackageEntry>> GetInstalledPackages(PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<PackageEntry> GetInstalledPackage(string fullName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<PackageEntry> GetInstalledPackageByFamilyName(string familyName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<List<User>> GetUsersForPackage(PackageEntry packageEntry, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<List<PackageEntry>> GetModificationPackages(string packageFullName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<AppxPackage> GetByIdentity(string packageName, PackageQuerySourceType mode = PackageQuerySourceType.InstalledForCurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<AppxPackage> GetByManifestPath(string manifestPath, PackageQuerySourceType mode = PackageQuerySourceType.InstalledForCurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}