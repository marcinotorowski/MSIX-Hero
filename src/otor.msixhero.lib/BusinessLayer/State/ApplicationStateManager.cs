using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>
    {
        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManagerFactory packageManagerFactory,
            IBusyManager busyManager,
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = new CommandExecutor(this, packageManagerFactory, interactionService, busyManager);
            this.EventAggregator = eventAggregator;
            this.CurrentState.LocalSettings.ShowSidebar = configurationService.GetCurrentConfiguration().List.Sidebar.Visible;
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }
    }
}