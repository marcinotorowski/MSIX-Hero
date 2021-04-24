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
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach
{
    public enum AppAttachVolumeType
    {
        Vhd,
        Cim,
        Vhdx
    }
    
    public interface IAppAttachManager : ISelfElevationAware
    {
        [Obsolete]
        Task CreateVolume(
            string packagePath, 
            string volumePath, 
            uint vhdSize, 
            bool extractCertificate,
            bool generateScripts, 
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progressReporter = null);

        Task CreateVolumes(
            IReadOnlyCollection<string> packagePaths,
            string volumeDirectory,
            AppAttachVolumeType type = AppAttachVolumeType.Vhd,
            bool extractCertificate = false,
            bool generateScripts = true,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null);
        
        Task CreateVolume(
            string packagePath, 
            string volumePath,
            uint size,
            AppAttachVolumeType type = AppAttachVolumeType.Vhd,
            bool extractCertificate = false,
            bool generateScripts = true, 
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progressReporter = null);
    }
}
