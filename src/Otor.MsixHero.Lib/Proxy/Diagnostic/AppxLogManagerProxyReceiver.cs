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
