using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Registry;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    // ReSharper disable once IdentifierTypo
    internal class DismountRegistryCommandExecutor : CommandExecutor
    {
        private readonly DismountRegistry action;
        private readonly ISelfElevationManagerFactory<IRegistryManager> packageManagerFactory;

        // ReSharper disable once IdentifierTypo
        public DismountRegistryCommandExecutor(
            DismountRegistry action,
            ISelfElevationManagerFactory<IRegistryManager> packageManagerFactory,
            IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            progressData?.Report(new ProgressData(0, "Un-mounting registry..."));
            await manager.DismountRegistry(this.action.PackageName, cancellationToken, progressData).ConfigureAwait(false);
            progressData?.Report(new ProgressData(100, "Un-mounting registry..."));
        }
    }
}
