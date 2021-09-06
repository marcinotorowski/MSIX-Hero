using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SelectLogsHandler : RequestHandler<SelectLogCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SelectLogsHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SelectLogCommand request)
        {
            this.commandExecutor.ApplicationState.EventViewer.SelectedLog = request.SelectedLog;
        }
    }
}