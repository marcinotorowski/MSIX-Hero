using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class FindUsersReducer : SelfElevationReducer<List<User>>
    {
        private readonly FindUsers action;
        private readonly IAppxPackageManager packageManager;

        public FindUsersReducer(FindUsers action, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager applicationStateManager) : base(action, elevatedClient, applicationStateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override async Task<List<User>> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            // todo:
            if (!this.action.ForceElevation && !this.StateManager.CurrentState.IsElevated && !this.StateManager.CurrentState.IsSelfElevated)
            {
                // if there is no indication that we can run in UAC don't even try
                return null;
            }

            return await this.packageManager.GetUsersForPackage(this.action.FullProductId, cancellationToken, progressReporter).ConfigureAwait(false);
        }
    }
}
