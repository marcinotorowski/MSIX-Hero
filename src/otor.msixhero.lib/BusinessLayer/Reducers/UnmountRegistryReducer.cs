using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    // ReSharper disable once IdentifierTypo
    internal class UnmountRegistryReducer : BaseSelfElevationReducer<ApplicationState>
    {
        private readonly UnmountRegistry action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        // ReSharper disable once IdentifierTypo
        public UnmountRegistryReducer(
            UnmountRegistry action, 
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
            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Un-mounting registry...";
                var state = stateManager.CurrentState;
                if (this.action.RequiresElevation && !state.IsElevated)
                {
                    await this.SelfElevateAndExecute(this.action, cancellationToken);
                }
                else
                {
                    await this.packageManager.UnmountRegistry(this.action.PackageName);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
