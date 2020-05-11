using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class RunToolInPackageCommandExecutor : CommandExecutor
    {
        private readonly RunToolInPackage action;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public RunToolInPackageCommandExecutor(
            RunToolInPackage action, 
            ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory, 
            IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.packageManagerFactory.Get(this.action.AsAdmin ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.RunToolInContext(this.action.PackageFamilyName, this.action.AppId, this.action.ToolPath, this.action.Arguments, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}