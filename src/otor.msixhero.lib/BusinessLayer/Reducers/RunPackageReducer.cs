using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RunPackageReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly RunPackage action;

        public RunPackageReducer(RunPackage action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return packageManager.Run(this.action.ManifestPath, this.action.PackageFamilyName, cancellationToken);
        }
    }
}