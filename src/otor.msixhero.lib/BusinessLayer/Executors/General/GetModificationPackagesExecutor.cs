using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class GetModificationPackagesExecutor : CommandWithOutputExecutor<List<InstalledPackage>>
    {
        private readonly GetModificationPackages command;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public GetModificationPackagesExecutor(
            GetModificationPackages command,
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
                    packageSource = new List<InstalledPackage>(await packageManager.GetModificationPackages(command.FullPackageName, PackageFindMode.AllUsers, cancellationToken, progressReporter).ConfigureAwait(false));
                    break;

                case PackageContext.CurrentUser:
                    packageManager = await this.packageManagerFactory.Get(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
                    packageSource = new List<InstalledPackage>(await packageManager.GetModificationPackages(command.FullPackageName, PackageFindMode.CurrentUser, cancellationToken, progressReporter).ConfigureAwait(false));
                    break;

                default:
                    throw new NotSupportedException();
            }

            return packageSource;
        }
    }
}
