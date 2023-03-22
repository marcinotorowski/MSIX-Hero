using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetEventViewerFilterHandler : IRequestHandler<SetEventViewerFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetEventViewerFilterHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._commandExecutor = commandExecutor;
            this._configurationService = configurationService;
        }

        async Task IRequestHandler<SetEventViewerFilterCommand>.Handle(SetEventViewerFilterCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.EventViewer.SearchKey = request.SearchKey;
            this._commandExecutor.ApplicationState.EventViewer.Filter = request.Filter;

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Events ??= new EventsConfiguration();
            cleanConfig.Events.Filter ??= new EventsFilterConfiguration();
            cleanConfig.Events.Filter.Filter = request.Filter;

            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}