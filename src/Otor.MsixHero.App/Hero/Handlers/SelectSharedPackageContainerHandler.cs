using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectSharedPackageContainerHandler : RequestHandler<SelectSharedPackageContainerCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SelectSharedPackageContainerHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SelectSharedPackageContainerCommand request)
        {
            this.commandExecutor.ApplicationState.Containers.SelectedContainer = request.SelectedContainer;
        }
    }
}