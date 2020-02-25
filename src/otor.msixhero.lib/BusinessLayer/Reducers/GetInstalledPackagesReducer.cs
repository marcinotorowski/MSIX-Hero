using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetInstalledPackagesReducer : SelfElevationReducer<List<InstalledPackage>>, IFinalizingReducer<List<InstalledPackage>>
    {
        private readonly GetPackages command;
        private readonly IAppxPackageManager packageManager;

        public GetInstalledPackagesReducer(
            GetPackages command,
            IElevatedClient elevatedClient,
            IAppxPackageManager packageManager,
            IWritableApplicationStateManager applicationStateManager) : base(command, elevatedClient, applicationStateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        protected override async Task<List<InstalledPackage>> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            List<InstalledPackage> packageSource;

            progressReporter?.Report(new ProgressData(0, "Just a moment..."));

            switch (command.Context)
            {
                case PackageContext.AllUsers:
                    packageSource = new List<InstalledPackage>(await this.packageManager.GetInstalledPackages(PackageFindMode.AllUsers, cancellationToken, progressReporter));
                    break;

                case PackageContext.CurrentUser:
                    packageSource = new List<InstalledPackage>(await this.packageManager.GetInstalledPackages(PackageFindMode.CurrentUser, cancellationToken, progressReporter));
                    break;

                default:
                    throw new NotSupportedException();
            }

            return packageSource;
        }

        async Task IFinalizingReducer<List<InstalledPackage>>.Finish(List<InstalledPackage> packageSource, CancellationToken cancellationToken)
        {
            var state = this.StateManager.CurrentState;
            var selectedPackageNames = new HashSet<string>(state.Packages.SelectedItems.Select(item => item.PackageId));

            state.Packages.SelectedItems.Clear();
            state.Packages.VisibleItems.Clear();
            state.Packages.HiddenItems.Clear();
            state.Packages.VisibleItems.AddRange(packageSource);

            state.Packages.Context = this.command.Context;
            this.StateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Publish(new PackagesCollectionChangedPayLoad(state.Packages.Context, CollectionChangeType.Reset));
            await this.StateManager.CommandExecutor.ExecuteAsync(new SetPackageFilter(state.Packages.Filter, state.Packages.SearchKey), cancellationToken).ConfigureAwait(false);
            await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(state.Packages.VisibleItems.Where(item => selectedPackageNames.Contains(item.PackageId)).ToList()), cancellationToken).ConfigureAwait(false);
        }
    }
}
