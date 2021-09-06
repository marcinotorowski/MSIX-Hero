using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SetSidebarVisibilityHandler : AsyncRequestHandler<SetPackageSidebarVisibilityCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;

        public SetSidebarVisibilityHandler(IMsixHeroCommandExecutor commandExecutor, IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.commandExecutor = commandExecutor;
        }

        protected override async Task Handle(SetPackageSidebarVisibilityCommand request, CancellationToken cancellationToken)
        {
            this.commandExecutor.ApplicationState.Packages.ShowSidebar = request.IsVisible;

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
            cleanConfig.Packages ??= new PackagesConfiguration();
            cleanConfig.Packages.Sidebar ??= new SidebarListConfiguration();
            cleanConfig.Packages.Sidebar.Visible = this.commandExecutor.ApplicationState.Packages.ShowSidebar;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
        }
    }
}