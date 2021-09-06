using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class OpenEventViewerHandler : AsyncRequestHandler<OpenEventViewerCommand>
    {
        private readonly ISelfElevationProxyProvider<IAppxLogManager> logManagerProvider;

        public OpenEventViewerHandler(ISelfElevationProxyProvider<IAppxLogManager> logManagerProvider)
        {
            this.logManagerProvider = logManagerProvider;
        }

        protected override Task Handle(OpenEventViewerCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task OpenEventViewer(OpenEventViewerCommand command, CancellationToken cancellationToken, IProgress<ProgressData> progressData)
        {
            var manager = await this.logManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            await manager.OpenEventViewer(command.Type, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}