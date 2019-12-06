using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.UI;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageSidebarVisibilityReducer : BaseReducer
    {
        private readonly SetPackageSidebarVisibility action;

        public SetPackageSidebarVisibilityReducer(SetPackageSidebarVisibility action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            if (this.action.IsVisible == this.StateManager.CurrentState.Packages.ShowSidebar)
            {
                return Task.FromResult(false);
            }

            this.StateManager.CurrentState.Packages.ShowSidebar = action.IsVisible;
            this.StateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Publish(action.IsVisible);
            return Task.FromResult(true);
        }
    }
}
