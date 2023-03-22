using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Tools;
using Otor.MsixHero.App.Hero.Executor;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetToolFilterHandler : IRequestHandler<SetToolFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SetToolFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }

        Task IRequestHandler<SetToolFilterCommand>.Handle(SetToolFilterCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Tools.SearchKey = request.SearchKey;
            return Task.CompletedTask;
        }
    }
}