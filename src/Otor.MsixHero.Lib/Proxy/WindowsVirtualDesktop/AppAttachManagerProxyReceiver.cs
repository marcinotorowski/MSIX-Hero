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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto;

namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop
{
    public class AppAttachManagerProxyReceiver : SelfElevationProxyReceiver<IAppAttachManager>
    {
        public AppAttachManagerProxyReceiver(IAppAttachManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(CreateVolumeDto);
        }

        public override Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            throw new InvalidOperationException("This command does not return anything.");
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var proxiedObject = (CreateVolumeDto)command;

            if (proxiedObject.VhdName != null && proxiedObject.Packages.Length < 2)
            {
                return this.SelfElevationAwareObject.CreateVolume(
                    proxiedObject.Packages.First(),
                    Path.Combine(proxiedObject.VhdDirectory, proxiedObject.VhdName), 
                    proxiedObject.SizeInMegaBytes,
                    proxiedObject.Type, 
                    proxiedObject.ExtractCertificate, 
                    proxiedObject.GenerateScripts, 
                    cancellationToken, 
                    progress);
            }

            return this.SelfElevationAwareObject.CreateVolumes(
                proxiedObject.Packages,
                proxiedObject.VhdDirectory,
                proxiedObject.Type,
                proxiedObject.ExtractCertificate,
                proxiedObject.GenerateScripts,
                cancellationToken,
                progress);
        }
    }
}
