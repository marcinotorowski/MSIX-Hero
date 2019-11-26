using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly MountRegistry action;
        private readonly IBusyManager busyManager;

        public MountRegistryReducer(
            MountRegistry action,
            IApplicationStateManager<ApplicationState> stateManager, 
            IBusyManager busyManager) : base(action, stateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = busyManager.Begin();
            try
            {
                context.Message = "Mounting registry...";
                await packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true, cancellationToken);
            }
            finally
            {
                busyManager.End(context);
            }
        }
    }
}
