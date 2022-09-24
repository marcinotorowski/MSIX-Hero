using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetContainersSortingHandler : AsyncRequestHandler<SetSharedPackageContainersSortingCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetContainersSortingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
        }
        protected override async Task Handle(SetSharedPackageContainersSortingCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.Containers.SortMode = request.SortMode;

            if (request.Descending.HasValue)
            {
                this.commandExecutor.ApplicationState.Containers.SortDescending = request.Descending.Value;
            }
            else
            {
                this.commandExecutor.ApplicationState.Containers.SortDescending = !this.commandExecutor.ApplicationState.Containers.SortDescending;
            }

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Containers ??= new ContainersConfiguration();
            cleanConfig.Containers.Sorting ??= new ContainersSortConfiguration();
            cleanConfig.Containers.Sorting.SortingMode = this.commandExecutor.ApplicationState.Containers.SortMode;
            cleanConfig.Containers.Sorting.Descending = this.commandExecutor.ApplicationState.Containers.SortDescending;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}