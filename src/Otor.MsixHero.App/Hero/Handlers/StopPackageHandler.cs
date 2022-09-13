using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Elevation;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class StopPackageHandler : AsyncRequestHandler<StopPackageCommand>
    {
        private readonly IUacElevation uacElevation;

        public StopPackageHandler(IUacElevation uacElevation)
        {
            this.uacElevation = uacElevation;
        }

        protected override Task Handle(StopPackageCommand request, CancellationToken cancellationToken)
        {
            return this.uacElevation.AsHighestAvailable<IAppxPackageManagerService>().Stop(request.Package.PackageFullName, cancellationToken);
        }
    }
}