using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class CheckForUpdatesHandler : IRequestHandler<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>
    {
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        public CheckForUpdatesHandler(ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider)
        {
            this.packageManagerProvider = packageManagerProvider;
        }

        public async Task<AppInstallerUpdateAvailabilityResult> Handle(CheckForUpdatesCommand request, CancellationToken cancellationToken)
        {
            var manager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            return await manager.CheckForUpdates(request.PackageFullName, cancellationToken).ConfigureAwait(false);
        }
    }
}