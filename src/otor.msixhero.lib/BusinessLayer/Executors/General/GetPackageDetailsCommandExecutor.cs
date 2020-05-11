using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class GetPackageDetailsCommandExecutor : CommandWithOutputExecutor<AppxPackage>
    {
        private readonly GetPackageDetails command;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public GetPackageDetailsCommandExecutor(GetPackageDetails command, ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task<AppxPackage> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var context = command.Context == PackageContext.CurrentUser
                ? PackageFindMode.CurrentUser
                : PackageFindMode.AllUsers;

            var manager = await this.packageManagerFactory.Get(this.command.Context == PackageContext.AllUsers ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (Uri.TryCreate(this.command.Source, UriKind.Absolute, out _))
            {
                return await manager.GetByManifestPath(this.command.Source, context, cancellationToken, progressData).ConfigureAwait(false);
            }

            return await manager.GetByIdentity(this.command.Source, context, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
