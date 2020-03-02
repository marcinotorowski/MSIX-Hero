using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class DeprovisionReducer : SelfElevationReducer
    {
        private readonly Deprovision action;
        private readonly IAppxPackageManager packageManager;

        public DeprovisionReducer(
            Deprovision action,
            IElevatedClient elevatedClient,
            IAppxPackageManager packageManager,
            IWritableApplicationStateManager stateManager) : base(action, elevatedClient, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override async Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (this.action.PackageFamilyName == null)
            {
                return;
            }

            var pkg = this.StateManager.CurrentState.Packages.VisibleItems.Union(this.StateManager.CurrentState.Packages.HiddenItems).FirstOrDefault(p => p.PackageFamilyName == this.action.PackageFamilyName);
            await this.packageManager.Deprovision(this.action.PackageFamilyName, cancellationToken: cancellationToken, progress: progress).ConfigureAwait(false);
            if (pkg != null)
            {
                await this.packageManager.Remove(new InstalledPackage[] {pkg}, false, false, cancellationToken);
            }
        }
    }
}
