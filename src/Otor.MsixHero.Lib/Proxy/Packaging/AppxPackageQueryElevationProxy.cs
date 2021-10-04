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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageQueryElevationProxy : IAppxPackageQuery
    {
        private readonly IElevatedClient client;

        public AppxPackageQueryElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }
        
        public async Task<List<User>> GetUsersForPackage(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetUsersForPackageDto
            {
                Source = package.PackageId
            };

            return await this.client.Get(proxyObject, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetUsersForPackageDto
            {
                Source = packageName
            };

            return await this.client.Get(proxyObject, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task<List<InstalledPackage>> GetInstalledPackages(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetInstalledPackagesDto
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public async Task<List<InstalledPackage>> GetModificationPackages(string packageFullName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (mode == PackageFindMode.Auto)
            {
                if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                {
                    mode = PackageFindMode.AllUsers;
                }
                else
                {
                    mode = PackageFindMode.CurrentUser;
                }
            }

            var proxyObject = new GetModificationPackagesDto
            {
                FullPackageName = packageFullName,
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers
            };

            return await this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task<AppxPackage> GetByIdentity(string packageName, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetByIdentityDto
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers,
                Source = packageName
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task<AppxPackage> GetByManifestPath(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetByManifestPathDto
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers,
                Source = manifestPath
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }
    }
}