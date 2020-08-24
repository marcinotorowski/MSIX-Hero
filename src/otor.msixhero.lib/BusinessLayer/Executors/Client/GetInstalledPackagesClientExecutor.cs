using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    internal class GetInstalledPackagesClientExecutor : CommandWithOutputExecutor<List<InstalledPackage>>
    {
        private readonly GetPackages command;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public GetInstalledPackagesClientExecutor(
            GetPackages command,
            ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory,
            IWritableApplicationStateManager applicationStateManager) : base(command, applicationStateManager)
        {
            this.command = command;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task<List<InstalledPackage>> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            List<InstalledPackage> packageSource;

            progressReporter?.Report(new ProgressData(0, "Just a moment..."));

            IAppxPackageManager packageManager;
            switch (command.Context)
            {
                case PackageContext.AllUsers:
                    packageManager = await this.packageManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
                    packageSource = new List<InstalledPackage>(await packageManager.GetInstalledPackages(PackageFindMode.AllUsers, cancellationToken, progressReporter).ConfigureAwait(false));
                    break;

                case PackageContext.CurrentUser:
                    packageManager = await this.packageManagerFactory.Get(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
                    packageSource = new List<InstalledPackage>(await packageManager.GetInstalledPackages(PackageFindMode.CurrentUser, cancellationToken, progressReporter).ConfigureAwait(false));
                    break;

                default:
                    throw new NotSupportedException();
            }

            this.StateManager.CurrentState.Packages.Context = command.Context;

            var state = this.StateManager.CurrentState;
            var selectedPackageNames = new HashSet<string>(state.Packages.SelectedItems.Select(item => item.PackageId));

            state.Packages.SelectedItems.Clear();
            state.Packages.VisibleItems.Clear();
            state.Packages.HiddenItems.Clear();
            state.Packages.VisibleItems.AddRange(packageSource);

            state.Packages.Context = this.command.Context;
            this.StateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Publish(new PackagesCollectionChangedPayLoad(state.Packages.Context, CollectionChangeType.Reset));
            await this.StateManager.CommandExecutor.ExecuteAsync(new SetPackageFilter(state.Packages.Filter, state.Packages.SearchKey, state.Packages.AddonsFilter), cancellationToken).ConfigureAwait(false);
            await this.StateManager.CommandExecutor.ExecuteAsync(new SelectPackages(state.Packages.VisibleItems.Where(item => selectedPackageNames.Contains(item.PackageId)).ToList()), cancellationToken).ConfigureAwait(false);

            return packageSource;
        }
    }
}
