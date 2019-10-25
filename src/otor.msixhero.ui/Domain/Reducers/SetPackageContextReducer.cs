using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.Events;
using MSI_Hero.Domain.State;
using MSI_Hero.Domain.State.Enums;
using MSI_Hero.Services;
using otor.msihero.lib;

namespace MSI_Hero.Domain.Reducers
{
    internal class SetPackageContextReducer : IReducer<ApplicationState>
    {
        private readonly SetPackageContext action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        public SetPackageContextReducer(SetPackageContext action, IAppxPackageManager packageManager, IBusyManager busyManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
        }

        public async Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var state = stateManager.CurrentState;

            if (state.Packages.Context == action.Context && !this.action.ForceReload)
            {
                return false;
            }

            var context = this.busyManager.Begin();
            try
            {
                IList<Package> packageSource;

                switch (action.Context)
                {
                    case PackageContext.Admin:
                        packageSource = await this.packageManager.GetPackages(PackageFindMode.AllUsers);
                        break;

                    case PackageContext.CurrentUser:
                        packageSource = await this.packageManager.GetPackages(PackageFindMode.CurrentUser);
                        break;

                    default:
                        throw new NotSupportedException();
                }

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