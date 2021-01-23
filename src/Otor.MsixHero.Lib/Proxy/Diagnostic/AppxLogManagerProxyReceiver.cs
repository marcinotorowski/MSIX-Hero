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
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic
{
    public class AppxLogManagerProxyReceiver : SelfElevationProxyReceiver<IAppxLogManager>
    {
        public AppxLogManagerProxyReceiver(IAppxLogManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(OpenEventViewerDto);
            yield return typeof(GetLogsDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is GetLogsDto getLogsDto)
            {
                object proxiedObject = await this.SelfElevationAwareObject.GetLogs(getLogsDto.MaxCount, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult) proxiedObject;
            }

            throw new NotSupportedException();
        }

        public override async Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is OpenEventViewerDto openEventViewerDto)
            {
                await this.SelfElevationAwareObject.OpenEventViewer(openEventViewerDto.Type, cancellationToken, progress).ConfigureAwait(false);
                return;
            }

            throw new NotSupportedException();
        }
    }
}
