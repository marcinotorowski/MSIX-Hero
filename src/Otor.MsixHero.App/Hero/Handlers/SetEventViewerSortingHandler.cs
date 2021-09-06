using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetEventViewerSortingHandler : AsyncRequestHandler<SetEventViewerSortingCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetEventViewerSortingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
        }
        protected override async Task Handle(SetEventViewerSortingCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.EventViewer.SortMode = request.SortMode;

            if (request.Descending.HasValue)
            {
                this.commandExecutor.ApplicationState.EventViewer.SortDescending = request.Descending.Value;
            }
            else
            {
                this.commandExecutor.ApplicationState.EventViewer.SortDescending = !this.commandExecutor.ApplicationState.EventViewer.SortDescending;
            }

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Events ??= new EventsConfiguration();
            cleanConfig.Events.Sorting ??= new EventsSortConfiguration();
            cleanConfig.Events.Sorting.SortingMode = this.commandExecutor.ApplicationState.EventViewer.SortMode;
            cleanConfig.Events.Sorting.Descending = this.commandExecutor.ApplicationState.EventViewer.SortDescending;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}