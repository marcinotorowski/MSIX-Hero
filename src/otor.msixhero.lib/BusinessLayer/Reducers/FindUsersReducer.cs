using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class FindUsersReducer : SelfElevationReducer<ApplicationState, FoundUsers>
    {
        private readonly FindUsers action;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public FindUsersReducer(
            FindUsers action,
            IApplicationStateManager<ApplicationState> applicationStateManager,
            IClientCommandRemoting clientCommandRemoting) : base(action, applicationStateManager, clientCommandRemoting)
        {
            this.action = action;
            this.clientCommandRemoting = clientCommandRemoting;
        }
        
        public override async Task<FoundUsers> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            var state = this.StateManager.CurrentState;
            if (!state.IsElevated && this.action.ForceElevation)
            {
                var result = await this.clientCommandRemoting.GetClientInstance().GetExecuted(this.action, cancellationToken).ConfigureAwait(false);
                this.StateManager.CurrentState.HasSelfElevated = true;
                return result;
            }

            var installedOn = new FoundUsers();

            if (!state.IsElevated)
            {
                installedOn.Status = ElevationStatus.ElevationRequired;
            }
            else
            {
                installedOn.Users = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetUsersOfPackage(), cancellationToken).ConfigureAwait(false);
            }

            return installedOn;
        }
    }
}
