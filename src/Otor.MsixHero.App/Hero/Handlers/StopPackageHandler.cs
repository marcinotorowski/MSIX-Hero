using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class StopPackageHandler : AsyncRequestHandler<StopPackageCommand>
    {
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        public StopPackageHandler(ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider)
        {
            this.packageManagerProvider = packageManagerProvider;
        }

        protected override async Task Handle(StopPackageCommand request, CancellationToken cancellationToken)
        {
            var manager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            await manager.Stop(request.Package.PackageId, cancellationToken).ConfigureAwait(false);
        }
    }
}