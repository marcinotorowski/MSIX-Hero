using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    // ReSharper disable once IdentifierTypo
    internal class UnmountRegistryReducer : SelfElevationReducer
    {
        private readonly UnmountRegistry action;
        private readonly IBusyManager busyManager;

        // ReSharper disable once IdentifierTypo
        public UnmountRegistryReducer(
            UnmountRegistry action,
            IWritableApplicationStateManager stateManager,
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
                await packageManager.UnmountRegistry(this.action.PackageName, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
