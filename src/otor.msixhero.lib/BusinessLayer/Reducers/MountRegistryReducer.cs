using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
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

        public MountRegistryReducer(
            MountRegistry action, 
            IAppxPackageManager packageManager, 
            IBusyManager busyManager,
            IProcessManager processManager) : base(processManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
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
                    await this.SelfElevateAndExecute(this.action, cancellationToken);
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
