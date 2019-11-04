using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackagesReducer : SelfElevationReducer<ApplicationState, List<Package>>
    {
        private readonly GetPackages action;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;
        private readonly IAppxPackageManager packageManager;

        public GetPackagesReducer(
            GetPackages action, 
            IApplicationStateManager<ApplicationState> applicationStateManager,
            IAppxPackageManager packageManager, 
            IBusyManager busyManager,
            IClientCommandRemoting clientCommandRemoting) : base(action, applicationStateManager)
        {
            this.action = action;
            this.busyManager = busyManager;
            this.clientCommandRemoting = clientCommandRemoting;
            this.packageManager = packageManager;
        }

        public override async Task<List<Package>> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin();
            try
            {
                List<Package> packageSource;

                context.Message = "Getting the list of packages...";
                if (this.action.RequiresElevation && !this.StateManager.CurrentState.IsElevated)
                {
                    packageSource = await this.clientCommandRemoting.GetClientInstance().GetExecuted(this.action, cancellationToken).ConfigureAwait(false);
                    this.StateManager.CurrentState.HasSelfElevated = true;
                }
                else
                {
                    switch (action.Context)
                    {
                        case PackageContext.AllUsers:
                            packageSource = new List<Package>(await this.packageManager.Get(PackageFindMode.AllUsers));
                            break;

                        case PackageContext.CurrentUser:
                            packageSource = new List<Package>(await this.packageManager.Get(PackageFindMode.CurrentUser));
                            break;

                        default:
                            throw new NotSupportedException();
                    }
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
