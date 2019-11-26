using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetRegistryMountStateReducer : SelfElevationReducer<ApplicationState, RegistryMountState>
    {
        private readonly GetRegistryMountState action;

        public GetRegistryMountStateReducer(GetRegistryMountState action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task<RegistryMountState> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return packageManager.GetRegistryMountState(this.action.InstallLocation, this.action.PackageName, cancellationToken);
        }
    }
}
