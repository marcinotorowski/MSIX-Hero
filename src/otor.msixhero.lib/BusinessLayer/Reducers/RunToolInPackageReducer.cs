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
        private readonly IBusyManager busyManager;

        public RunToolInPackageReducer(RunToolInPackage action, IWritableApplicationStateManager stateManager, IBusyManager busyManager) : base(action, stateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();
            try
            {
                await packageManager.RunToolInContext(this.action.PackageFamilyName, this.action.AppId, this.action.ToolPath, this.action.Arguments, cancellationToken, context);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}