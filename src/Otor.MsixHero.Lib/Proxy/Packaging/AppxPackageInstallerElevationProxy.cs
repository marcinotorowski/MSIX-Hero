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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageInstallerElevationProxy : IAppxPackageInstaller
    {
        private readonly IElevatedClient client;

        public AppxPackageInstallerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }

        public Task Deprovision(string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DeprovisionDto
            {
                PackageFamilyName = packageFamilyName
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public async Task<bool> IsInstalled(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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

            var proxyObject = new CheckIfInstalledDto
            {
                ManifestFilePath = manifestPath,
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers
            };

            return await this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task Remove(IReadOnlyCollection<InstalledPackage> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var proxyObject = new RemoveDto
            {
                Packages = packages.ToList(),
                Context = forAllUsers ? PackageContext.AllUsers : PackageContext.CurrentUser,
                RemoveAppData = !preserveAppData
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Remove(IReadOnlyCollection<string> packageFullNames, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var proxyObject = new RemoveCurrentUserDto
            {
                PackageFullNames = packageFullNames.ToList(),
                RemoveAppData = !preserveAppData
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Add(string filePath, AddAppxPackageOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new AddDto
            {
                FilePath = filePath,
                AllUsers = options.HasFlag(AddAppxPackageOptions.AllUsers),
                AllowDowngrade = options.HasFlag(AddAppxPackageOptions.AllowDowngrade),
                KillRunningApps = options.HasFlag(AddAppxPackageOptions.KillRunningApps)
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }
    }
}