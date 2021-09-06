using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetPackageFilterHandler : AsyncRequestHandler<SetPackageFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetPackageFilterHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
        }

        protected override async Task Handle(SetPackageFilterCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.Packages.Filter = request.Filter;
            this.commandExecutor.ApplicationState.Packages.SearchKey = request.SearchKey;

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Filter ??= new PackagesFilterConfiguration();
            cleanConfig.Packages.Filter.Filter = request.Filter;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}