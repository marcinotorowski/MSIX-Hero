using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class DeprovisionReducer : SelfElevationReducer
    {
        private readonly Deprovision action;
        private readonly IBusyManager busyManager;

        public DeprovisionReducer(Deprovision action, IWritableApplicationStateManager stateManager, IBusyManager busyManager) : base(action, stateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager,
CancellationToken cancellationToken = default)
        {
            if (this.action.PackageFamilyName == null)
            {
                return;
            }
            
            var context = this.busyManager.Begin();

            var myProgress = new Progress();
            // ReSharper disable once ConvertToLocalFunction
            EventHandler<ProgressData> handler = (sender, data) =>
            {
                context.Progress = data.Progress;
                context.Message = data.Message;
            };

            myProgress.ProgressChanged += handler;

            try
            {
                await packageManager.Deprovision(this.action.PackageFamilyName, cancellationToken: cancellationToken, progress: myProgress).ConfigureAwait(false);
                await this.StateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.StateManager.CurrentState.Packages.Context), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                myProgress.ProgressChanged -= handler;
                this.busyManager.End(context);
            }
        }
    }
}
