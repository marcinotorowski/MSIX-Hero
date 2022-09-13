using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class CheckForUpdatesHandler : IRequestHandler<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>
    {
        private readonly IAppxPackageManagerService _packageManagerService;

        public CheckForUpdatesHandler(IAppxPackageManagerService packageManagerService)
        {
            this._packageManagerService = packageManagerService;
        }

        public Task<AppInstallerUpdateAvailabilityResult> Handle(CheckForUpdatesCommand request, CancellationToken cancellationToken)
        {
            return this._packageManagerService.CheckForUpdates(request.PackageFullName, cancellationToken);
        }
    }
}