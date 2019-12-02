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
    internal class FindUsersReducer : SelfElevationReducer<ApplicationState, List<User>>
    {
        private readonly FindUsers action;

        public FindUsersReducer(
            FindUsers action,
            IApplicationStateManager<ApplicationState> applicationStateManager) : base(action, applicationStateManager)
        {
            this.action = action;
        }
        
        public override async Task<List<User>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            if (!this.action.ForceElevation && !this.StateManager.CurrentState.IsElevated && !this.StateManager.CurrentState.IsSelfElevated)
            {
                // if there is no indication that we can run in UAC don't even try
                return null;
            }

            return await packageManager.GetUsersForPackage(this.action.FullProductId, cancellationToken).ConfigureAwait(false);
        }
    }
}
