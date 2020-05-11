using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    public class AddPackageCommandExecutor : CommandExecutor
    {
        private readonly AddPackage command;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public AddPackageCommandExecutor(
            AddPackage command,
            ISelfElevationManagerFactory<IAppxPackageManager> packageManager, 
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.packageManagerFactory = packageManager;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            AddPackageOptions opts = 0;

            if (this.command.AllUsers)
            {
                opts |= AddPackageOptions.AllUsers;
            }

            if (this.command.KillRunningApps)
            {
                opts |= AddPackageOptions.KillRunningApps;
            }

            if (this.command.AllowDowngrade)
            {
                opts |= AddPackageOptions.AllowDowngrade;
            }

            var packageManager = await this.packageManagerFactory.Get(this.command.AllUsers ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await packageManager.Add(this.command.FilePath, opts, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
