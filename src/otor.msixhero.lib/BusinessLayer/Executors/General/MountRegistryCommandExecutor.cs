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
    internal class MountRegistryCommandExecutor : CommandExecutor
    {
        private readonly MountRegistry action;
        private readonly ISelfElevationManagerFactory<IRegistryManager> registryManagerFactory;

        public MountRegistryCommandExecutor(
            MountRegistry action, 
            ISelfElevationManagerFactory<IRegistryManager> registryManagerFactory, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.registryManagerFactory = registryManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.registryManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
