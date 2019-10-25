using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : BaseReducer<ApplicationState>
    {
        private readonly MountRegistry action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        public MountRegistryReducer(MountRegistry action, IAppxPackageManager packageManager, IBusyManager busyManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
        }

        public override async Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = busyManager.Begin();
            try
            {
                context.Message = "Mounting registry...";
                var state = stateManager.CurrentState;
                if (this.action.RequiresElevation && !state.IsElevated)
                {
                    return await this.SelfElevateAndExecute<MountRegistry, bool>(this.action, cancellationToken);
                }

                await this.packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true);
                return true;
            }
            finally
            {
                busyManager.End(context);
            }
        }
    }
}
