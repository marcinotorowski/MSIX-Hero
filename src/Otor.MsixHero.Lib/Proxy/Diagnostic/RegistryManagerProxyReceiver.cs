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
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Diagnostic.Dto;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic
{
    public class RegistryManagerProxyReceiver : SelfElevationProxyReceiver<IRegistryManager>
    {
        public RegistryManagerProxyReceiver(IRegistryManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(DismountRegistryDto);
            yield return typeof(GetRegistryMountStateDto);
            yield return typeof(MountRegistryDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is GetRegistryMountStateDto getRegistryMountStateDto)
            {
                object proxiedResult = await this.SelfElevationAwareObject.GetRegistryMountState(getRegistryMountStateDto.InstallLocation, getRegistryMountStateDto.PackageName, cancellationToken).ConfigureAwait(false);
                return (TCommandResult)proxiedResult;
            }

            throw new NotSupportedException();
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is DismountRegistryDto dismountRegistryDto)
            {
                return this.SelfElevationAwareObject.DismountRegistry(dismountRegistryDto.PackageName, cancellationToken, progress);
            }

            if (command is MountRegistryDto mountRegistryDto)
            {
                return this.SelfElevationAwareObject.DismountRegistry(mountRegistryDto.PackageName, cancellationToken, progress);
            }

            throw new NotSupportedException();
        }
    }
}
