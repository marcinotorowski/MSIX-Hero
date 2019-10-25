using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class ReloadPackagesReducer : BaseReducer<ApplicationState>
    {
        private readonly GetPackages action;
        private readonly IBusyManager busyManager;
        private readonly IAppxPackageManager packageManager;

        public ReloadPackagesReducer(GetPackages action, IAppxPackageManager packageManager, IBusyManager busyManager)
        {
            this.action = action;
            this.busyManager = busyManager;
            this.packageManager = packageManager;
        }

        public override async Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin();
            try
            {
                IList<Package> packageSource;
                
                if (this.action.RequiresElevation && !stateManager.CurrentState.IsElevated)
                {
                    var client = new Client();
                    packageSource = await client.Execute<GetPackages, List<Package>>(this.action, cancellationToken);
                }
                else
                {
                    switch (action.Context)
                    {
                        case PackageContext.AllUsers:
                            packageSource = await this.packageManager.GetPackages(PackageFindMode.AllUsers);
                            break;

                        case PackageContext.CurrentUser:
                            packageSource = await this.packageManager.GetPackages(PackageFindMode.CurrentUser);
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }


                var state = stateManager.CurrentState;
                var selectedPackageNames = new HashSet<string>(state.Packages.SelectedItems.Select(item => item.Name));

                state.Packages.SelectedItems.Clear();
                state.Packages.VisibleItems.Clear();
                state.Packages.HiddenItems.Clear();
                state.Packages.VisibleItems.AddRange(packageSource);

                stateManager.EventAggregator.GetEvent<PackagesLoadedEvent>().Publish(state.Packages.Context);
                await stateManager.Executor.ExecuteAsync(new SetPackageFilter(state.Packages.Filter, state.Packages.SearchKey), cancellationToken).ConfigureAwait(false);
                await stateManager.Executor.ExecuteAsync(new SelectPackages(state.Packages.VisibleItems.Where(item => selectedPackageNames.Contains(item.Name)).ToList()), cancellationToken).ConfigureAwait(false);

                state.Packages.Context = this.action.Context;

                return true;
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
