using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Executor;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectEventHandler : IRequestHandler<SelectEventCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SelectEventHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }
        
        Task IRequestHandler<SelectEventCommand>.Handle(SelectEventCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.EventViewer.SelectedAppxEvent = request.SelectedAppxEvent;
            return Task.CompletedTask;
        }
    }
}