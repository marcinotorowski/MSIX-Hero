using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetPackagesHandler : IRequestHandler<GetPackagesCommand, IList<InstalledPackage>>, IObserver<ActivePackageFullNames>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IBusyManager busyManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IRunningAppsDetector detector;
        private readonly ISelfElevationProxyProvider<IAppxPackageQuery> packageQueryProvider;

        public GetPackagesHandler(
            IMsixHeroCommandExecutor commandExecutor,
            IBusyManager busyManager,
            IEventAggregator eventAggregator,
            IRunningAppsDetector detector,
            ISelfElevationProxyProvider<IAppxPackageQuery> packageQueryProvider)
        {
            detector.Subscribe(this);
            this.commandExecutor = commandExecutor;
            this.busyManager = busyManager;
            this.eventAggregator = eventAggregator;
            this.detector = detector;
            this.packageQueryProvider = packageQueryProvider;
        }

        public async Task<IList<InstalledPackage>> Handle(GetPackagesCommand request, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin(OperationType.PackageLoading);
            try
            {
                this.detector.StartListening();

                var level = SelfElevationLevel.HighestAvailable;

                if (request.FindMode == PackageFindMode.AllUsers)
                {
                    level = SelfElevationLevel.AsAdministrator;
                }

                var manager = await this.packageQueryProvider.GetProxyFor(level, cancellationToken).ConfigureAwait(false);

                PackageFindMode mode;
                if (request.FindMode.HasValue)
                {
                    mode = request.FindMode.Value;
                    if (mode == PackageFindMode.Auto)
                    {
                        var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken);
                        mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
                    }
                }
                else
                {
                    switch (this.commandExecutor.ApplicationState.Packages.Mode)
                    {
                        case PackageContext.CurrentUser:
                            mode = PackageFindMode.CurrentUser;
                            break;
                        case PackageContext.AllUsers:
                            mode = PackageFindMode.AllUsers;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                var selected = this.commandExecutor.ApplicationState.Packages.SelectedPackages.Select(p => p.ManifestLocation).ToArray();
                var results = await manager.GetInstalledPackages(mode, cancellationToken, context).ConfigureAwait(false);

                // this.packageListSynchronizer.EnterWriteLock();
                this.commandExecutor.ApplicationState.Packages.AllPackages.Clear();
                this.commandExecutor.ApplicationState.Packages.AllPackages.AddRange(results);

                foreach (var item in this.detector.GetCurrentlyRunning(this.commandExecutor.ApplicationState.Packages.AllPackages))
                {
                    item.IsRunning = true;
                }

                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        this.commandExecutor.ApplicationState.Packages.Mode = PackageContext.CurrentUser;
                        break;
                    case PackageFindMode.AllUsers:
                        this.commandExecutor.ApplicationState.Packages.Mode = PackageContext.AllUsers;
                        break;
                }

                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        this.commandExecutor.ApplicationState.Packages.Mode = PackageContext.CurrentUser;
                        break;
                    case PackageFindMode.AllUsers:
                        this.commandExecutor.ApplicationState.Packages.Mode = PackageContext.AllUsers;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await this.commandExecutor.Invoke(this, new SelectPackagesCommand(selected), cancellationToken).ConfigureAwait(false);

                return results;
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
        
        void IObserver<ActivePackageFullNames>.OnCompleted()
        {
        }

        void IObserver<ActivePackageFullNames>.OnError(Exception error)
        {
        }

        void IObserver<ActivePackageFullNames>.OnNext(ActivePackageFullNames value)
        {
            this.commandExecutor.ApplicationState.Packages.ActivePackageNames = value.Running;
            this.eventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Publish(value);
        }
    }
}