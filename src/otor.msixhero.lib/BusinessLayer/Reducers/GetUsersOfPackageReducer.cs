using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Users;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class GetUsersOfPackageReducer : SelfElevationReducer<ApplicationState, List<User>>
    {
        private readonly GetUsersOfPackage action;
        private readonly IAppxPackageManager packageManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public GetUsersOfPackageReducer(
            GetUsersOfPackage action, 
            IApplicationStateManager<ApplicationState> applicationStateManager,
            IAppxPackageManager packageManager,
            IClientCommandRemoting clientCommandRemoting) : base(action, applicationStateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.clientCommandRemoting = clientCommandRemoting;
        }

        public override async Task<List<User>> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            var state = this.StateManager.CurrentState;
            if (!state.IsElevated)
            {
                var result = await this.clientCommandRemoting.GetClientInstance().GetExecuted(this.action, cancellationToken).ConfigureAwait(false);
                this.StateManager.CurrentState.HasSelfElevated = true;
                return result;
            }
            
            return await this.packageManager.GetUsersForPackage(this.action.FullProductId).ConfigureAwait(false);
        }
    }
}
