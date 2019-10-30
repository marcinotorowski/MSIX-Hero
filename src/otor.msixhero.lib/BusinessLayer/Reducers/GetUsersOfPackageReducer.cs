using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class GetUsersOfPackageReducer : BaseSelfElevationWithOutputReducer<ApplicationState, List<User>>
    {
        private readonly GetUsersOfPackage action;
        private readonly IAppxPackageManager packageManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public GetUsersOfPackageReducer(
            GetUsersOfPackage action, 
            IAppxPackageManager packageManager,
            IClientCommandRemoting clientCommandRemoting)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.clientCommandRemoting = clientCommandRemoting;
        }

        public override async Task<List<User>> ReduceAndOutputAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var state = stateManager.CurrentState;
            if (!state.IsElevated)
            {
                var result = await this.clientCommandRemoting.Execute(this.action, cancellationToken).ConfigureAwait(false);
                stateManager.CurrentState.HasSelfElevated = true;
                return result;
            }

            return await this.packageManager.GetUsersForPackage(this.action.FullProductId).ConfigureAwait(false);

        }
    }
}
