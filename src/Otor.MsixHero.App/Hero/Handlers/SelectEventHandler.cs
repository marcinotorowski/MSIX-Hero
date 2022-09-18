using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Executor;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectEventHandler : RequestHandler<SelectEventCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SelectEventHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SelectEventCommand request)
        {
            this.commandExecutor.ApplicationState.EventViewer.SelectedAppxEvent = request.SelectedAppxEvent;
        }
    }
}