using MediatR;
using Otor.MsixHero.App.Hero.Commands.Tools;
using Otor.MsixHero.App.Hero.Executor;

namespace Otor.MsixHero.App.Hero.Handlers
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
            this.commandExecutor.ApplicationState.Tools.SearchKey = request.SearchKey;
        }
    }
}