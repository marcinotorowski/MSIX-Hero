using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.Events;
using MSI_Hero.Domain.State;

namespace MSI_Hero.Domain.Reducers
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
