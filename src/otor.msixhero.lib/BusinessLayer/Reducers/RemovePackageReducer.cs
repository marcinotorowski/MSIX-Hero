using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class RemovePackageReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly RemovePackages action;
        private readonly IBusyManager busyManager;

        public RemovePackageReducer(RemovePackages action, IApplicationStateManager<ApplicationState> stateManager, IBusyManager busyManager) : base(action, stateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager,
CancellationToken cancellationToken = default)
        {
            if (this.action.Packages == null || !this.action.Packages.Any())
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

            var eventData = new PackagesCollectionChangedPayLoad(this.action.Context, CollectionChangeType.Simple);
            try
            {
                await packageManager.Remove(this.action.Packages, cancellationToken: cancellationToken, progress: myProgress).ConfigureAwait(false);
                foreach (var item in this.action.Packages)
                {
                    eventData.OldPackages.Add(item);
                }

                await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(this.action.Packages, SelectionMode.RemoveFromSelection), cancellationToken);
                this.StateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Publish(eventData);
            }
            finally
            {
                myProgress.ProgressChanged -= handler;
                this.busyManager.End(context);
            }
        }
    }
}
