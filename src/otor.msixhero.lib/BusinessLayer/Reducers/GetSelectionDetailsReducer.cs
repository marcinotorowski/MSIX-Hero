using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetSelectionDetailsReducer : SelfElevationReducer<ApplicationState, SelectionDetails>
    {
        private readonly GetSelectionDetails action;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public GetSelectionDetailsReducer(
            GetSelectionDetails action,
            IApplicationStateManager<ApplicationState> applicationStateManager,
            IClientCommandRemoting clientCommandRemoting) : base(action, applicationStateManager)
        {
            this.action = action;
            this.clientCommandRemoting = clientCommandRemoting;
        }
        
        public override async Task<SelectionDetails> GetReduced(CancellationToken cancellationToken)
        {
            var state = this.StateManager.CurrentState;
            if (!state.IsElevated && this.action.ForceElevation)
            {
                var result = await this.clientCommandRemoting.GetClientInstance().GetExecuted(this.action, cancellationToken).ConfigureAwait(false);
                this.StateManager.CurrentState.HasSelfElevated = true;
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
                var ttt = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetLogs(5), cancellationToken).ConfigureAwait(false);
                installedOn.Users = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetUsersOfPackage(), cancellationToken).ConfigureAwait(false);
            }

            return details;
        }
    }
}
