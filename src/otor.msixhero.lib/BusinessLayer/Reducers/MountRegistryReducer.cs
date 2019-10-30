using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : BaseSelfElevationReducer<ApplicationState>
    {
        private readonly MountRegistry action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public MountRegistryReducer(
            MountRegistry action, 
            IAppxPackageManager packageManager, 
            IBusyManager busyManager,
            IClientCommandRemoting clientCommandRemoting)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
            this.clientCommandRemoting = clientCommandRemoting;
        }

        public override async Task ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = busyManager.Begin();
            try
            {
                context.Message = "Mounting registry...";
                var state = stateManager.CurrentState;
                if (this.action.RequiresElevation && !state.IsElevated)
                {
                    await this.clientCommandRemoting.Execute(this.action, cancellationToken);
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
