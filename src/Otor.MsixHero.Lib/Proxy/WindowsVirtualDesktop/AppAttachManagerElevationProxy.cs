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
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto;

namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop
{
    public class AppAttachManagerElevationProxy : IAppAttachManager
    {
        private readonly IElevatedClient client;

        public AppAttachManagerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }

        public Task CreateVolume(string packagePath, string volumePath, uint size, AppAttachVolumeType type = AppAttachVolumeType.Vhd,
            bool extractCertificate = false, bool generateScripts = true, CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null)
        {
            var cmd = new CreateVolumeDto(packagePath, volumePath)
            {
                ExtractCertificate = extractCertificate,
                GenerateScripts = generateScripts,
                SizeInMegaBytes = size,
                Type = type
            };

            return client.Invoke(cmd, cancellationToken, progressReporter);
        }

        public Task CreateVolumes(IReadOnlyCollection<string> packagePaths, string volumeDirectory,
            AppAttachVolumeType type = AppAttachVolumeType.Vhd, bool extractCertificate = false, bool generateScripts = true,
            CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            var cmd = new CreateVolumeDto(packagePaths, volumeDirectory)
            {
                ExtractCertificate = extractCertificate,
                GenerateScripts = generateScripts,
                Type = type
            };

            return client.Invoke(cmd, cancellationToken, progressReporter);
        }

        [Obsolete]
        public Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool extractCertificate, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            var cmd = new CreateVolumeDto(packagePath, volumePath)
            {
                ExtractCertificate = extractCertificate, 
                GenerateScripts = generateScripts,
                Type = AppAttachVolumeType.Vhd,
                SizeInMegaBytes = vhdSize
            };

            return client.Invoke(cmd, cancellationToken, progressReporter);
        }
    }
}
