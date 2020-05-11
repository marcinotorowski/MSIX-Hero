using System;
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
    internal class RunPackageCommandExecutor : CommandExecutor
    {
        private readonly RunPackage action;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public RunPackageCommandExecutor(RunPackage action, ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.Run(this.action.ManifestPath, this.action.PackageFamilyName, this.action.ApplicationId, cancellationToken, progressReporter).ConfigureAwait(false);
        }
    }
}