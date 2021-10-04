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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageRunnerElevationProxy : IAppxPackageRunner
    {
        private readonly IElevatedClient client;

        public AppxPackageRunnerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }
        
        public async Task RunToolInContext(InstalledPackage package, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using IAppxFileReader reader = new FileInfoFileReaderAdapter(package.ManifestLocation);

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
    }
}