using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class CheckForUpdatesHandler : IRequestHandler<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>
    {
        private readonly IAppxPackageManager _packageManagerProvider;

        public CheckForUpdatesHandler(IAppxPackageManager packageManagerProvider)
        {
            this._packageManagerProvider = packageManagerProvider;
        }

        public Task<AppInstallerUpdateAvailabilityResult> Handle(CheckForUpdatesCommand request, CancellationToken cancellationToken)
        {
            return this._packageManagerProvider.CheckForUpdates(request.PackageFullName, cancellationToken);
        }
    }
}