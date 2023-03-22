using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetPackageGroupingHandler : IRequestHandler<SetPackageGroupingCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetPackageGroupingHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._commandExecutor = commandExecutor;
            this._configurationService = configurationService;
        }

        async Task IRequestHandler<SetPackageGroupingCommand>.Handle(SetPackageGroupingCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Packages.GroupMode = request.GroupMode;

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Group ??= new PackagesGroupConfiguration();
            cleanConfig.Packages.Group.GroupMode = this._commandExecutor.ApplicationState.Packages.GroupMode;
            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}