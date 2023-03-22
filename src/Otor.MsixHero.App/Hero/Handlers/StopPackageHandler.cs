using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Elevation;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class StopPackageHandler : IRequestHandler<StopPackageCommand>
    {
        private readonly IUacElevation _uacElevation;

        public StopPackageHandler(IUacElevation uacElevation)
        {
            this._uacElevation = uacElevation;
        }

        Task IRequestHandler<StopPackageCommand>.Handle(StopPackageCommand request, CancellationToken cancellationToken)
        {
            return this._uacElevation.AsHighestAvailable<IAppxPackageManagerService>().Stop(request.PackageEntry.PackageFullName, cancellationToken);
        }
    }
}