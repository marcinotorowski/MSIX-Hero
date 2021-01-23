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
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic
{
    public class AppxLogManagerElevationProxy : IAppxLogManager
    {
        private readonly IElevatedClient client;

        public AppxLogManagerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }

        public Task OpenEventViewer(EventLogCategory type, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new OpenEventViewerDto
            {
                Type = type
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
    }
}