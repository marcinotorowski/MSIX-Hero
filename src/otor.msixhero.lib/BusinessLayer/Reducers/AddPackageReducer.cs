using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : BaseReducer<ApplicationState>
    {
        private readonly AddPackage command;
        private readonly IBusyManager busyManager;

        public AddPackageReducer(AddPackage command, IApplicationStateManager<ApplicationState> state, IBusyManager busyManager) : base(command, state)
        {
            this.command = command;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var appxReader = await AppxManifestSummaryBuilder.FromMsix(this.command.FilePath);

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
