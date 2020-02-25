using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetRegistryMountStateReducer : BaseReducer<RegistryMountState>
    {
        private readonly GetRegistryMountState action;
        private readonly IAppxPackageManager packageManager;

        public GetRegistryMountStateReducer(GetRegistryMountState action, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        public override Task<RegistryMountState> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            return this.packageManager.GetRegistryMountState(this.action.InstallLocation, this.action.PackageName, cancellationToken, progressReporter);
        }
    }
}
