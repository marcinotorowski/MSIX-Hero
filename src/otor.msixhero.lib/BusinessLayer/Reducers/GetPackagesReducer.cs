using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackagesReducer : BaseSelfElevationWithOutputReducer<ApplicationState, List<Package>>
    {
        private readonly GetPackages action;
        private readonly IBusyManager busyManager;
        private readonly IAppxPackageManager packageManager;

        public GetPackagesReducer(
            GetPackages action, 
            IAppxPackageManager packageManager, 
            IBusyManager busyManager,
            IProcessManager processManager) : base(processManager)
        {
            this.action = action;
            this.busyManager = busyManager;
            this.packageManager = packageManager;
        }

        public override Task ReduceAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
        {
            return this.ReduceAndOutputAsync(state, cancellationToken);
        }

        public override async Task<List<Package>> ReduceAndOutputAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin();
            try
            {
                List<Package> packageSource;
                
                if (this.action.RequiresElevation && !stateManager.CurrentState.IsElevated)
                {
                    packageSource = await this.GetOutputFromSelfElevation(this.action, cancellationToken).ConfigureAwait(false);
                    stateManager.CurrentState.HasSelfElevated = true;
                }
                else
                {
                    switch (action.Context)
                    {
                        case PackageContext.AllUsers:
                            packageSource = new List<Package>(await this.packageManager.GetPackages(PackageFindMode.AllUsers));
                            break;

                        case PackageContext.CurrentUser:
                            packageSource = new List<Package>(await this.packageManager.GetPackages(PackageFindMode.CurrentUser));
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
                await stateManager.CommandExecutor.ExecuteAsync(new SetPackageFilter(state.Packages.Filter, state.Packages.SearchKey), cancellationToken).ConfigureAwait(false);
                await stateManager.CommandExecutor.ExecuteAsync(new SelectPackages(state.Packages.VisibleItems.Where(item => selectedPackageNames.Contains(item.Name)).ToList()), cancellationToken).ConfigureAwait(false);

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
