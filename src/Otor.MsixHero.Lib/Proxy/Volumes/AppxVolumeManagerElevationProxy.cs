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
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Volumes.Dto;

namespace Otor.MsixHero.Lib.Proxy.Volumes
{
    public class AppxVolumeManagerElevationProxy : IAppxVolumeManager
    {
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory;
        private readonly IElevatedClient client;

        public AppxVolumeManagerElevationProxy(IElevatedClient client, ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory)
        {
            this.volumeManagerFactory = volumeManagerFactory;
            this.client = client;
        }

        public Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetAllDto();
            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public async Task<AppxVolume> GetVolumeForPath(string path, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetVolumeForPath(path, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetDefault(cancellationToken, progress).ConfigureAwait(false);
        }

        public Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new AddDto
            {
                DrivePath = drivePath
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DeleteDto
            {
                Name = volume.Name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Delete(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DeleteDto
            {
                Name = name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new MountDto
            {
                Name = volume.Name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DismountDto
            {
                Name = volume.Name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Mount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new MountDto
            {
                Name = name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Dismount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DismountDto
            {
                Name = name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new SetDefaultDto
            {
                DrivePath = volume.PackageStorePath
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new SetDefaultDto
            {
                DrivePath = drivePath
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public async Task<List<AppxVolume>> GetAvailableDrivesForAppxVolume(bool onlyUnused, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetAvailableDrivesForAppxVolume(onlyUnused, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task MovePackageToVolume(string volumePackagePath, string packageFullName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new MovePackageToVolumeDto(volumePackagePath, packageFullName);
            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task MovePackageToVolume(AppxVolume volume, AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (volume == null)
            {
                throw new ArgumentNullException(nameof(volume));
            }
            
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            return this.MovePackageToVolume(volume.Name, package.FullName, cancellationToken, progress);
        }
    }
}