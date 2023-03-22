using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectSharedPackageContainerHandler : IRequestHandler<SelectSharedPackageContainerCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SelectSharedPackageContainerHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }

        Task IRequestHandler<SelectSharedPackageContainerCommand>.Handle(SelectSharedPackageContainerCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Containers.SelectedContainer = request.SelectedContainer;
            return Task.CompletedTask;
        }
    }
}