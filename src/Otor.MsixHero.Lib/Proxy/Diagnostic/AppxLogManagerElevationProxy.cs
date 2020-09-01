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