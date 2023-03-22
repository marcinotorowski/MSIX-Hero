using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetContainersSortingHandler : IRequestHandler<SetSharedPackageContainersSortingCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetContainersSortingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._commandExecutor = commandExecutor;
            this._configurationService = configurationService;
        }
        async Task IRequestHandler<SetSharedPackageContainersSortingCommand>.Handle(SetSharedPackageContainersSortingCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Containers.SortMode = request.SortMode;

            if (request.Descending.HasValue)
            {
                this._commandExecutor.ApplicationState.Containers.SortDescending = request.Descending.Value;
            }
            else
            {
                this._commandExecutor.ApplicationState.Containers.SortDescending = !this._commandExecutor.ApplicationState.Containers.SortDescending;
            }

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Containers ??= new ContainersConfiguration();
            cleanConfig.Containers.Sorting ??= new ContainersSortConfiguration();
            cleanConfig.Containers.Sorting.SortingMode = this._commandExecutor.ApplicationState.Containers.SortMode;
            cleanConfig.Containers.Sorting.Descending = this._commandExecutor.ApplicationState.Containers.SortDescending;
            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}