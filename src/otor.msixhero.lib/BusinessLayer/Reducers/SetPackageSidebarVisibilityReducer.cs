using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.UI;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageSidebarVisibilityReducer : BaseReducer<ApplicationState>
    {
        private readonly SetPackageSidebarVisibility action;

        public SetPackageSidebarVisibilityReducer(SetPackageSidebarVisibility action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            if (this.action.IsVisible == this.StateManager.CurrentState.LocalSettings.ShowSidebar)
            {
                return Task.FromResult(false);
            }

            this.StateManager.CurrentState.LocalSettings.ShowSidebar = action.IsVisible;
            this.StateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Publish(action.IsVisible);
            return Task.FromResult(true);
        }
    }
}
