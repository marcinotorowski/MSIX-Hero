using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetSelectionDetailsReducer : BaseSelfElevationWithOutputReducer<ApplicationState, SelectionDetails>
    {
        private readonly GetSelectionDetails action;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public GetSelectionDetailsReducer(
            GetSelectionDetails action, 
            IClientCommandRemoting clientCommandRemoting)
        {
            this.action = action;
            this.clientCommandRemoting = clientCommandRemoting;
        }
        
        public override async Task<SelectionDetails> ReduceAndOutputAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var state = stateManager.CurrentState;
            if (!state.IsElevated && this.action.ForceElevation)
            {
                var result = await this.clientCommandRemoting.Execute(this.action, cancellationToken).ConfigureAwait(false);
                stateManager.CurrentState.HasSelfElevated = true;
                return result;
            }

            var details = new SelectionDetails();
            var installedOn = new InstalledOnDetails();
            details.InstalledOn = installedOn;

            if (!state.IsElevated)
            {
                installedOn.Status = ElevationStatus.ElevationRequired;
            }
            else
            {
                installedOn.Users = await stateManager.CommandExecutor.ExecuteAsync<List<User>>(new GetUsersOfPackage(), cancellationToken).ConfigureAwait(false);
            }

            return details;
        }
    }
}
