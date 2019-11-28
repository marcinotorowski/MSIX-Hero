using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class FindUsersReducer : SelfElevationReducer<ApplicationState, FoundUsers>
    {
        private readonly FindUsers action;

        public FindUsersReducer(
            FindUsers action,
            IApplicationStateManager<ApplicationState> applicationStateManager) : base(action, applicationStateManager)
        {
            this.action = action;
        }
        
        public override async Task<FoundUsers> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var elevation = this.action.ForceElevation || this.StateManager.CurrentState.IsElevated || this.StateManager.CurrentState.IsSelfElevated
                ? ElevationStatus.OK
                : ElevationStatus.ElevationRequired;

            var users = await packageManager.GetUsersForPackage(this.action.FullProductId, cancellationToken).ConfigureAwait(false);
            
            var foundUsers = new FoundUsers
            {
                Status = elevation,
                Users = new List<User>(users)
            };
            return foundUsers;
        }
    }
}
