using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Registry;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class GetRegistryMountStateCommandExecutor : CommandWithOutputExecutor<RegistryMountState>
    {
        private readonly GetRegistryMountState action;
        private readonly ISelfElevationManagerFactory<IRegistryManager> packageManagerFactory;

        public GetRegistryMountStateCommandExecutor(GetRegistryMountState action, ISelfElevationManagerFactory<IRegistryManager> packageManagerFactory, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task<RegistryMountState> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetRegistryMountState(this.action.InstallLocation, this.action.PackageName, cancellationToken, progressReporter).ConfigureAwait(false);
        }
    }
}
