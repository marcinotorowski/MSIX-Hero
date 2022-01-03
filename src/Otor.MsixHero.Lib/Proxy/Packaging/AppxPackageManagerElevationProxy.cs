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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
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

        public async Task<AppInstallerUpdateAvailabilityResult> CheckForUpdates(string itemPackageId, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new CheckUpdateAvailabilityDto(itemPackageId);

            return await this.client.Get(proxyObject, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task Stop(string packageFullName, CancellationToken cancellationToken = default)
        {
            var proxyObject = new StopDto
            {
                PackageFullName = packageFullName
            };

            return this.client.Invoke(proxyObject, cancellationToken);
        }

        public Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetLogsDto
            {
                MaxCount = maxCount
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

    }
}