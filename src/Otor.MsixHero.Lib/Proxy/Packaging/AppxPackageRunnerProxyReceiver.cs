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
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageRunnerProxyReceiver : SelfElevationProxyReceiver<IAppxPackageRunner>
    {
        public AppxPackageRunnerProxyReceiver(IAppxPackageRunner selfElevationAware) : base(selfElevationAware)
        {
        }


        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(RunDto);
            yield return typeof(RunToolInContextDto);
        }

        public override Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            throw new NotSupportedException();
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            switch (command)
            {
                case RunDto runPackageDto:
                {
                    return this.SelfElevationAwareObject.Run(runPackageDto.ManifestPath, runPackageDto.ApplicationId, cancellationToken, progress);
                }

                case RunToolInContextDto runToolInContextDto:
                {
                    return this.SelfElevationAwareObject.RunToolInContext(runToolInContextDto.PackageFamilyName, runToolInContextDto.AppId, runToolInContextDto.ToolPath, runToolInContextDto.Arguments, cancellationToken, progress);
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}