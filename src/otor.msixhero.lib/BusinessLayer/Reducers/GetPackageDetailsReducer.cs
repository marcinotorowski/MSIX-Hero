using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackageDetailsReducer : SelfElevationReducer<AppxPackage>
    {
        private readonly GetPackageDetails command;
        private readonly IAppxPackageManager packageManager;

        public GetPackageDetailsReducer(GetPackageDetails command, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        protected override Task<AppxPackage> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            return this.packageManager.Get(this.command.PackageFullName, command.Context == PackageContext.CurrentUser ? PackageFindMode.CurrentUser : PackageFindMode.AllUsers, cancellationToken, progressReporter);
        }
    }
}
