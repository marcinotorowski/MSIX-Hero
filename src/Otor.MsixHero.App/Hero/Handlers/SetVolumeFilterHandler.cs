using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetVolumeFilterHandler : IRequestHandler<SetVolumeFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SetVolumeFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }

        Task IRequestHandler<SetVolumeFilterCommand>.Handle(SetVolumeFilterCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Volumes.SearchKey = request.SearchKey;
            return Task.CompletedTask;
        }
    }
}