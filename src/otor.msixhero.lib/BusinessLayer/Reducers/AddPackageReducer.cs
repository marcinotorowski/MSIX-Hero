using System;
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
            var appxReader = await AppxManifestSummaryBuilder.FromFile(this.command.FilePath);

            var context = this.busyManager.Begin();
            try
            {
                await this.busyManager.Execute(progress => packageManager.Add(this.command.FilePath, cancellationToken, progress));

                var allPackages = await this.StateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.StateManager.CurrentState.Packages.Context), cancellationToken).ConfigureAwait(false);
                var selected = allPackages.FirstOrDefault(p => p.Name == appxReader.Name);

                await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(selected), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
