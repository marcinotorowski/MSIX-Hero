using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetPackageSortingHandler : AsyncRequestHandler<SetPackageSortingCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetPackageSortingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
        }

        protected override async Task Handle(SetPackageSortingCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.Packages.SortMode = request.SortMode;

            if (request.Descending.HasValue)
            {
                this.commandExecutor.ApplicationState.Packages.SortDescending = request.Descending.Value;
            }
            else
            {
                this.commandExecutor.ApplicationState.Packages.SortDescending = !this.commandExecutor.ApplicationState.Packages.SortDescending;
            }

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Sorting ??= new PackagesSortConfiguration();
            cleanConfig.Packages.Sorting.SortingMode = this.commandExecutor.ApplicationState.Packages.SortMode;
            cleanConfig.Packages.Sorting.Descending = this.commandExecutor.ApplicationState.Packages.SortDescending;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}