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
    public class OpenEventViewerHandler : IRequestHandler<OpenEventViewerCommand>
    {
        private readonly IUacElevation _uacElevation;

        public OpenEventViewerHandler(IUacElevation uacElevation)
        {
            this._uacElevation = uacElevation;
        }

        Task IRequestHandler<OpenEventViewerCommand>.Handle(OpenEventViewerCommand request, CancellationToken cancellationToken)
        {
            return this.OpenEventViewer(request, cancellationToken, null);
        }

        private Task OpenEventViewer(OpenEventViewerCommand command, CancellationToken cancellationToken, IProgress<ProgressData> progressData)
        {
            return this._uacElevation.AsAdministrator<IAppxEventService>().OpenEventViewer(command.Type, cancellationToken, progressData);
        }
    }
}