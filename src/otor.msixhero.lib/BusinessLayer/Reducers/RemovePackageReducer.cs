using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class RemovePackageReducer : SelfElevationReducer, IFinalizingReducer
    {
        private readonly RemovePackages action;
        private readonly IAppxPackageManager packageManager;

        public RemovePackageReducer(RemovePackages action, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(action, elevatedClient, stateManager)
        {
            this.action = action;
            this.packageManager = packageManager;
        }

        protected override async Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (this.action.Packages == null || !this.action.Packages.Any())
            {
                return;
            }
            
            await this.packageManager.Remove(this.action.Packages, cancellationToken: cancellationToken, progress: progress).ConfigureAwait(false);
        }

        public async Task Finish(CancellationToken cancellationToken = default)
        {
            var eventData = new PackagesCollectionChangedPayLoad(this.action.Context, CollectionChangeType.Simple);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var item in this.action.Packages)
            {
                if (item.IsProvisioned)
                {
                    continue;
                }

                eventData.OldPackages.Add(item);
            }

            if (!eventData.OldPackages.Any())
            {
                return;
            }

            await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(this.action.Packages, SelectionMode.RemoveFromSelection), cancellationToken).ConfigureAwait(false);
            this.StateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Publish(eventData);
        }
    }
}
