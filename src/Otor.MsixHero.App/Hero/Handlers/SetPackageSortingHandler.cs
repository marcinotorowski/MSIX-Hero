using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetPackageSortingHandler : IRequestHandler<SetPackageSortingCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetPackageSortingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._commandExecutor = commandExecutor;
            this._configurationService = configurationService;
        }

        async Task IRequestHandler<SetPackageSortingCommand>.Handle(SetPackageSortingCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Packages.SortMode = request.SortMode;

            if (request.Descending.HasValue)
            {
                this._commandExecutor.ApplicationState.Packages.SortDescending = request.Descending.Value;
            }
            else
            {
                this._commandExecutor.ApplicationState.Packages.SortDescending = !this._commandExecutor.ApplicationState.Packages.SortDescending;
            }

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Sorting ??= new PackagesSortConfiguration();
            cleanConfig.Packages.Sorting.SortingMode = this._commandExecutor.ApplicationState.Packages.SortMode;
            cleanConfig.Packages.Sorting.Descending = this._commandExecutor.ApplicationState.Packages.SortDescending;
            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}