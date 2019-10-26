using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>, IApplicationStateManager
    {
        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManager packageManager,
            IBusyManager busyManager,
            IProcessManager processManager)
        {
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = new CommandExecutor(this, packageManager, busyManager, processManager);
            this.EventAggregator = eventAggregator;
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }
    }
}