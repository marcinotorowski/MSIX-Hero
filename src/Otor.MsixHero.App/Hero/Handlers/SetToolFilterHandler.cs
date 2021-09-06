using MediatR;
using Otor.MsixHero.App.Hero.Commands.Dashboard;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetToolFilterHandler : RequestHandler<SetToolFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SetToolFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SetToolFilterCommand request)
        {
            this.commandExecutor.ApplicationState.Dashboard.SearchKey = request.SearchKey;
        }
    }
}