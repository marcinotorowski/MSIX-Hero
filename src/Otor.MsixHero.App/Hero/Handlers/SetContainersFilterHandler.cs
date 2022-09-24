using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetContainersFilterHandler : RequestHandler<SetSharedPackageContainersFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SetContainersFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SetSharedPackageContainersFilterCommand request)
        {
            this.commandExecutor.ApplicationState.Containers.SearchKey = request.SearchKey;
        }
    }
}