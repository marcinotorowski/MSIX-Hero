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
    public class GetUsersOfPackageReducer : SelfElevationReducer<ApplicationState, List<User>>
    {
        private readonly GetUsersOfPackage action;

        public GetUsersOfPackageReducer(GetUsersOfPackage action, IApplicationStateManager<ApplicationState> applicationStateManager) : base(action, applicationStateManager)
        {
            this.action = action;
        }

        public override async Task<List<User>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return await packageManager.GetUsersForPackage(this.action.FullProductId, cancellationToken).ConfigureAwait(false);
        }
    }
}
