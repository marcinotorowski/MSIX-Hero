using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetVolumeFilterHandler : RequestHandler<SetVolumeFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SetVolumeFilterHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SetVolumeFilterCommand request)
        {
            this.commandExecutor.ApplicationState.Volumes.SearchKey = request.SearchKey;
        }
    }
}