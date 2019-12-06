using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RunToolInPackageReducer : SelfElevationReducer
    {
        private readonly RunToolInPackage action;
        
        public RunToolInPackageReducer(RunToolInPackage action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return packageManager.RunToolInContext(this.action.PackageFamilyName, this.action.AppId, this.action.ToolPath, this.action.Arguments, cancellationToken);
        }
    }
}