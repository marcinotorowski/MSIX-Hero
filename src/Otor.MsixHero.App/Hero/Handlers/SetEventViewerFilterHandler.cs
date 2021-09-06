using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetEventViewerFilterHandler : AsyncRequestHandler<SetEventViewerFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetEventViewerFilterHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
        }

        protected override async Task Handle(SetEventViewerFilterCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.EventViewer.SearchKey = request.SearchKey;
            this.commandExecutor.ApplicationState.EventViewer.Filter = request.Filter;

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Events ??= new EventsConfiguration();
            cleanConfig.Events.Filter ??= new EventsFilterConfiguration();
            cleanConfig.Events.Filter.Filter = request.Filter;

            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}