using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    // ReSharper disable once IdentifierTypo
    internal class UnmountRegistryReducer : BaseReducer<ApplicationState>
    {
        private readonly UnmountRegistry action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        // ReSharper disable once IdentifierTypo
        public UnmountRegistryReducer(UnmountRegistry action, IAppxPackageManager packageManager, IBusyManager busyManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
        }

        public override async Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Un-mounting registry...";
                var state = stateManager.CurrentState;
                if (this.action.RequiresElevation && !state.IsElevated)
                {
                    return await this.SelfElevateAndExecute<UnmountRegistry, bool>(this.action, cancellationToken);
                }

                await this.packageManager.UnmountRegistry(this.action.PackageName);
                return true;
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
