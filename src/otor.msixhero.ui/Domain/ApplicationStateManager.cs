using MSI_Hero.Domain.State;
using MSI_Hero.Services;
using otor.msihero.lib;
using Prism.Events;

namespace MSI_Hero.Domain
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>, IApplicationStateManager
    {
        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManager packageManager,
            IBusyManager busyManager)
        {
            this.CurrentState = new ApplicationState(eventAggregator);
            this.Executor = new ActionExecutor(this, packageManager, busyManager);
            this.EventAggregator = eventAggregator;
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public IActionExecutor Executor { get; }

        public IEventAggregator EventAggregator { get; }
    }
}