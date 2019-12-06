using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : SelfElevationReducer
    {
        private readonly MountRegistry action;

        public MountRegistryReducer(MountRegistry action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            await packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true, cancellationToken);
        }
    }
}
