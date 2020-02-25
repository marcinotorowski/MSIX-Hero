using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RunToolInPackageReducer : SelfElevationReducer
    {
        private readonly RunToolInPackage action;
        private readonly IAppxPackageManager packageManager;

        public RunToolInPackageReducer(RunToolInPackage action, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(action, elevatedClient, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.packageManager.RunToolInContext(this.action.PackageFamilyName, this.action.AppId, this.action.ToolPath, this.action.Arguments, cancellationToken, progress);
        }
    }
}