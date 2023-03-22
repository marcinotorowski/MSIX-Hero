using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetSidebarVisibilityHandler : IRequestHandler<SetPackageSidebarVisibilityCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IConfigurationService _configurationService;

        public SetSidebarVisibilityHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this._configurationService = configurationService;
            this._commandExecutor = commandExecutor;
        }

        async Task IRequestHandler<SetPackageSidebarVisibilityCommand>.Handle(SetPackageSidebarVisibilityCommand request, CancellationToken cancellationToken)
        {
            this._commandExecutor.ApplicationState.Packages.ShowSidebar = request.IsVisible;

            var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Sidebar ??= new SidebarListConfiguration();
            cleanConfig.Packages.Sidebar.Visible = this._commandExecutor.ApplicationState.Packages.ShowSidebar;
            await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}