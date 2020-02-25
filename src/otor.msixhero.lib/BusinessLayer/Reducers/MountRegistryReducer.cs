using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : SelfElevationReducer
    {
        private readonly MountRegistry action;
        private readonly IAppxPackageManager packageManager;

        public MountRegistryReducer(MountRegistry action, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(action, elevatedClient, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override async Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            await this.packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
