using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.ui.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly MountRegistry action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public MountRegistryReducer(
            MountRegistry action,
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
            var context = busyManager.Begin();
            try
            {
                context.Message = "Mounting registry...";
                var state = this.StateManager.CurrentState;
                if (this.action.RequiresElevation && !state.IsElevated)
                {
                    await this.clientCommandRemoting.GetClientInstance().Execute(this.action, cancellationToken);
                    return;
                }

                await this.packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true);
            }
            finally
            {
                busyManager.End(context);
            }
        }
    }
}
