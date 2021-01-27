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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageManagerElevationProxy : IAppxPackageManager
    {
        private readonly IElevatedClient client;

        public AppxPackageManagerElevationProxy(IElevatedClient client)
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

        public async Task<AppInstallerUpdateAvailabilityResult> CheckForUpdates(string itemPackageId, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new CheckUpdateAvailabilityDto(itemPackageId);

            return await this.client.Get(proxyObject, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task Deprovision(string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DeprovisionDto
            {
                PackageFamilyName = packageFamilyName
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Stop(string packageFullName, CancellationToken cancellationToken = default)
        {
            var proxyObject = new StopDto
            {
                PackageFullName = packageFullName
            };

            return this.client.Invoke(proxyObject, cancellationToken);
        }

        public async Task RunToolInContext(InstalledPackage package, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using (IAppxFileReader reader = new FileInfoFileReaderAdapter(package.ManifestLocation))
            {
                var maniReader = new AppxManifestReader();
                var manifest = await maniReader.Read(reader, cancellationToken).ConfigureAwait(false);

                if (!manifest.Applications.Any())
                {
                    throw new InvalidOperationException("Cannot start tool on a package without applications.");
                }

                var proxyObject = new RunToolInContextDto
                {
                    PackageFamilyName = package.PackageFamilyName,
                    AppId = manifest.Applications[0].Id,
                    Arguments = arguments,
                    ToolPath = toolPath
                };

                await this.client.Invoke(proxyObject, cancellationToken, progress).ConfigureAwait(false);
            }
        }

        public Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new RunToolInContextDto
            {
                PackageFamilyName = packageFamilyName,
                AppId = appId,
                Arguments = arguments,
                ToolPath = toolPath
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Run(InstalledPackage package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new RunDto
            {
                ManifestPath = package.ManifestLocation,
                ApplicationId = appId
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Run(string packageManifestLocation, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new RunDto
            {
                ManifestPath = packageManifestLocation,
                ApplicationId = appId
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

        public Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetLogsDto
            {
                MaxCount = maxCount
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task Add(string filePath, AddAppxPackageOptions options = (AddAppxPackageOptions) 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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