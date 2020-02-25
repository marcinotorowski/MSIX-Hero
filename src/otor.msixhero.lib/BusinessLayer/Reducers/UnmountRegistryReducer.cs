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
    // ReSharper disable once IdentifierTypo
    internal class UnmountRegistryReducer : SelfElevationReducer
    {
        private readonly UnmountRegistry action;
        private readonly IAppxPackageManager packageManager;

        // ReSharper disable once IdentifierTypo
        public UnmountRegistryReducer(
            UnmountRegistry action,
            IElevatedClient elevatedClient,
            IAppxPackageManager packageManager,
            IWritableApplicationStateManager stateManager) : base(action, elevatedClient, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override async Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            progress?.Report(new ProgressData(0, "Un-mounting registry..."));
            await this.packageManager.UnmountRegistry(this.action.PackageName, cancellationToken).ConfigureAwait(false);
        }
    }
}
