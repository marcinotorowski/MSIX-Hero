using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    // ReSharper disable once IdentifierTypo
    internal class UnmountRegistryReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly UnmountRegistry action;
        private readonly IBusyManager busyManager;

        // ReSharper disable once IdentifierTypo
        public UnmountRegistryReducer(
            UnmountRegistry action,
            IApplicationStateManager<ApplicationState> stateManager,
            IBusyManager busyManager) : base(action, stateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }
        
        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Un-mounting registry...";
                await packageManager.UnmountRegistry(this.action.PackageName, cancellationToken);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
