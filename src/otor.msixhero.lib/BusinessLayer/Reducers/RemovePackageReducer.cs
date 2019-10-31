using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Composition.Interactions;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;
using otor.msixhero.ui.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class RemovePackageReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly RemovePackage action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public RemovePackageReducer(
            RemovePackage action,
            IApplicationStateManager<ApplicationState> stateManager,
            IAppxPackageManager packageManager, 
            IBusyManager busyManager, 
            IClientCommandRemoting clientCommandRemoting) : base(action, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
            this.clientCommandRemoting = clientCommandRemoting;
        }

        public override async Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken)
        {

            var context = this.busyManager.Begin();
            context.Progress = 30;
            context.Message = "Removing " + this.action.Package.DisplayName;

            try
            {
                if (this.action.RequiresElevation && !this.StateManager.CurrentState.IsElevated)
                {
                    await this.packageManager.Remove(this.action.Package).ConfigureAwait(false);
                }
                else
                {
                    await this.clientCommandRemoting.GetClientInstance().Execute(this.action, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
