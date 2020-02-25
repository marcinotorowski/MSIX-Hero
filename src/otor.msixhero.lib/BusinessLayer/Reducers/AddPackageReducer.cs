using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : BaseReducer
    {
        private readonly AddPackage command;
        private readonly IAppxPackageManager packageManager;

        public AddPackageReducer(
            AddPackage command, 
            IAppxPackageManager packageManager, 
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        public override async Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            AppxManifestSummary appxReader;
            if (!string.Equals(".appinstaller", Path.GetExtension(this.command.FilePath),  StringComparison.OrdinalIgnoreCase))
            {
                appxReader = await AppxManifestSummaryBuilder.FromFile(this.command.FilePath, AppxManifestSummaryBuilderMode.Identity).ConfigureAwait(false);
            }
            else
            {
                appxReader = null;
            }

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

            var progress = new WrappedProgress(progressData);
            var progress1 = progress.GetChildProgress(20);
            var progress2 = progress.GetChildProgress(20);
            await this.packageManager.Add(this.command.FilePath, opts, cancellationToken, progress1).ConfigureAwait(false);
            var allPackages = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.StateManager.CurrentState.Packages.Context), cancellationToken, progress2).ConfigureAwait(false);

            if (appxReader != null)
            {
                var selected = allPackages.FirstOrDefault(p => p.Name == appxReader.Name);
                await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(selected), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
