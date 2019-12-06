using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackagesReducer : SelfElevationReducer<List<Package>>
    {
        private readonly GetPackages action;
        private readonly IBusyManager busyManager;

        public GetPackagesReducer(
            GetPackages action,
            IWritableApplicationStateManager applicationStateManager,
            IBusyManager busyManager) : base(action, applicationStateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task<List<Package>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();
            try
            {
                List<Package> packageSource;

                context.Message = "Getting the list of packages...";
                
                switch (action.Context)
                {
                    case PackageContext.AllUsers:
                        packageSource = new List<Package>(await packageManager.Get(PackageFindMode.AllUsers, cancellationToken));
                        break;

                    case PackageContext.CurrentUser:
                        packageSource = new List<Package>(await packageManager.Get(PackageFindMode.CurrentUser, cancellationToken));
                        break;

                    default:
                        throw new NotSupportedException();
                }


                var state = this.StateManager.CurrentState;
                var selectedPackageNames = new HashSet<string>(state.Packages.SelectedItems.Select(item => item.Name));

                state.Packages.SelectedItems.Clear();
                state.Packages.VisibleItems.Clear();
                state.Packages.HiddenItems.Clear();
                state.Packages.VisibleItems.AddRange(packageSource);
                
                this.StateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Publish(new PackagesCollectionChangedPayLoad(state.Packages.Context, CollectionChangeType.Reset));
                await this.StateManager.CommandExecutor.ExecuteAsync(new SetPackageFilter(state.Packages.Filter, state.Packages.SearchKey), cancellationToken).ConfigureAwait(false);
                await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(state.Packages.VisibleItems.Where(item => selectedPackageNames.Contains(item.Name)).ToList()), cancellationToken).ConfigureAwait(false);

                state.Packages.Context = this.action.Context;

                return packageSource;
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
