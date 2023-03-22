using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetPackageFilterHandler : IRequestHandler<SetPackageFilterCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetPackageFilterHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._commandExecutor = commandExecutor;
            this._configurationService = configurationService;
        }

        async Task IRequestHandler<SetPackageFilterCommand>.Handle(SetPackageFilterCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Packages.Filter = request.Filter;
            this._commandExecutor.ApplicationState.Packages.SearchKey = request.SearchKey;

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Filter ??= new PackagesFilterConfiguration();
            cleanConfig.Packages.Filter.Filter = request.Filter;
            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}