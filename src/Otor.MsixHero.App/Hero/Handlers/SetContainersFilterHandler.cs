using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetContainersFilterHandler : IRequestHandler<SetSharedPackageContainersFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SetContainersFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }

        Task IRequestHandler<SetSharedPackageContainersFilterCommand>.Handle(SetSharedPackageContainersFilterCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Containers.SearchKey = request.SearchKey;
            return Task.CompletedTask;
        }
    }
}