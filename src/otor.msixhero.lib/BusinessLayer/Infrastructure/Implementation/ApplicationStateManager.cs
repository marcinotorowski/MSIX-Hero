using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.ui.Services;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>
    {
        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManager packageManager,
            IBusyManager busyManager,
            IInteractionService interactionService,
            IClientCommandRemoting clientCommandRemoting)
        {
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = new CommandExecutor(this, packageManager, interactionService, busyManager, clientCommandRemoting);
            this.EventAggregator = eventAggregator;
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }
    }
}