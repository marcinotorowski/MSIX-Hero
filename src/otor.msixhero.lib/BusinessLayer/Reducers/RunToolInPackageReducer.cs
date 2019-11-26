using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RunToolInPackageReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly RunToolInPackage action;
        
        public RunToolInPackageReducer(RunToolInPackage action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return packageManager.RunToolInContext(this.action.PackageFamilyName, this.action.AppId, this.action.ToolPath, this.action.Arguments, cancellationToken);
        }
    }
}