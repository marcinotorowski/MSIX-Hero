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
    internal class GetInstalledPackagesReducer : SelfElevationReducer<List<InstalledPackage>>
    {
        private readonly GetPackages action;
        private readonly IBusyManager busyManager;

        public GetInstalledPackagesReducer(
            GetPackages action,
            IWritableApplicationStateManager applicationStateManager,
            IBusyManager busyManager) : base(action, applicationStateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
        }

        public override async Task<List<InstalledPackage>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();
            try
            {
                List<InstalledPackage> packageSource;

                context.Message = "Just a moment...";
                
                switch (action.Context)
                {
                    case PackageContext.AllUsers:
                        packageSource = new List<InstalledPackage>(await packageManager.GetInstalledPackages(PackageFindMode.AllUsers, cancellationToken, context));
                        break;

                    case PackageContext.CurrentUser:
                        packageSource = new List<InstalledPackage>(await packageManager.GetInstalledPackages(PackageFindMode.CurrentUser, cancellationToken, context));
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
