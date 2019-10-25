using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageSidebarVisibilityReducer : IReducer<ApplicationState>
    {
        private readonly SetPackageSidebarVisibility action;

        public SetPackageSidebarVisibilityReducer(SetPackageSidebarVisibility action)
        {
            this.action = action;
        }

        public Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
        {
            if (this.action.IsVisible == state.CurrentState.LocalSettings.ShowSidebar)
            {
                return Task.FromResult(false);
            }

            state.CurrentState.LocalSettings.ShowSidebar = action.IsVisible;
            state.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Publish(action.IsVisible);
            return Task.FromResult(true);
        }
    }
}
