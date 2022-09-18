using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class OpenEventViewerHandler : AsyncRequestHandler<OpenEventViewerCommand>
    {
        private readonly IUacElevation uacElevation;

        public OpenEventViewerHandler(IUacElevation uacElevation)
        {
            this.uacElevation = uacElevation;
        }

        protected override Task Handle(OpenEventViewerCommand request, CancellationToken cancellationToken)
        {
            return this.OpenEventViewer(request, cancellationToken, null);
        }

        private Task OpenEventViewer(OpenEventViewerCommand command, CancellationToken cancellationToken, IProgress<ProgressData> progressData)
        {
            return this.uacElevation.AsAdministrator<IAppxEventService>().OpenEventViewer(command.Type, cancellationToken, progressData);
        }
    }
}