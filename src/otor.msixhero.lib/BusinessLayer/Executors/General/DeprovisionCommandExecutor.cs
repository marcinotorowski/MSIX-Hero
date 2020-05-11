using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    public class DeprovisionCommandExecutor : CommandExecutor
    {
        private readonly Deprovision action;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public DeprovisionCommandExecutor(
            Deprovision action,
            ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory,
            IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            if (this.action.PackageFamilyName == null)
            {
                return;
            }

            var pkg = this.StateManager.CurrentState.Packages.VisibleItems.Union(this.StateManager.CurrentState.Packages.HiddenItems).FirstOrDefault(p => p.PackageFamilyName == this.action.PackageFamilyName);

            using var cp = new WrappedProgress(progressData);

            var p1 = cp.GetChildProgress(50);
            var p2 = cp.GetChildProgress(50);

            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            p1.Report(new ProgressData(0, "De-provisioning..."));
            await manager.Deprovision(this.action.PackageFamilyName, cancellationToken, p1).ConfigureAwait(false);
            p1.Report(new ProgressData(100, "De-provisioning..."));

            if (pkg != null)
            {
                p2.Report(new ProgressData(0, "Removing for the current user..."));
                await manager.Remove(new[] {pkg}, false, false, cancellationToken, p2).ConfigureAwait(false);
                p2.Report(new ProgressData(100, "Removing for the current user..."));
            }
        }
    }
}
