using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Commands.Manager;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : BaseReducer
    {
        private readonly AddPackage command;
        private readonly IBusyManager busyManager;

        public AddPackageReducer(AddPackage command, IWritableApplicationStateManager stateManager, IBusyManager busyManager) : base(command, stateManager)
        {
            this.command = command;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
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

            var context = this.busyManager.Begin();
            try
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

                await this.busyManager.ExecuteAsync(progress => packageManager.Add(this.command.FilePath, opts, cancellationToken, progress)).ConfigureAwait(false);

                var allPackages = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.StateManager.CurrentState.Packages.Context), cancellationToken).ConfigureAwait(false);

                if (appxReader != null)
                {
                    var selected = allPackages.FirstOrDefault(p => p.Name == appxReader.Name);
                    await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(selected), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
