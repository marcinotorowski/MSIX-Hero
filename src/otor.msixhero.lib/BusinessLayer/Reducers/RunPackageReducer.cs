using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RunPackageReducer : BaseReducer
    {
        private readonly RunPackage action;
        private readonly IAppxPackageManager packageManager;

        public RunPackageReducer(RunPackage action, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            return this.packageManager.Run(this.action.ManifestPath, this.action.PackageFamilyName, this.action.ApplicationId, cancellationToken, progressReporter);
        }
    }
}