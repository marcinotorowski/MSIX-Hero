using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetPackagesHandler : IRequestHandler<GetPackagesCommand, IList<PackageEntry>>, IObserver<ActivePackageFullNames>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IBusyManager busyManager;
        private readonly IUacElevation uacElevation;
        private readonly IEventAggregator eventAggregator;
        private readonly IRunningAppsDetector detector;

        public GetPackagesHandler(
            IMsixHeroCommandExecutor commandExecutor,
            IBusyManager busyManager,
            IUacElevation uacElevation,
            IEventAggregator eventAggregator,
            IRunningAppsDetector detector)
        {
            detector.Subscribe(this);
            this.commandExecutor = commandExecutor;
            this.busyManager = busyManager;
            this.uacElevation = uacElevation;
            this.eventAggregator = eventAggregator;
            this.detector = detector;
        }

        public async Task<IList<PackageEntry>> Handle(GetPackagesCommand request, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin(OperationType.PackageLoading);
            try
            {
                var manager = request.FindMode == PackageFindMode.AllUsers ? this.uacElevation.AsAdministrator<IAppxPackageQueryService>() : this.uacElevation.AsHighestAvailable<IAppxPackageQueryService>();

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

                var selected = this.commandExecutor.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).ToList();
                
                var results = await manager.GetInstalledPackages(mode, cancellationToken, context).ConfigureAwait(false);
                
                // this.packageListSynchronizer.EnterWriteLock();
                this.commandExecutor.ApplicationState.Packages.AllPackages.Clear();
                this.commandExecutor.ApplicationState.Packages.AllPackages.AddRange(results);

                var currentlyRunning = new HashSet<string>(this.detector.GetCurrentlyRunningPackageFamilyNames(), StringComparer.Ordinal);

                foreach (var item in this.commandExecutor.ApplicationState.Packages.AllPackages)
                {
                    item.IsRunning = currentlyRunning.Contains(item.PackageFamilyName);
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

                // Just in case, make sure to remove stuff from the selection if the list of packages does not have it anymore.
                for (var i = selected.Count - 1; i >= 0; i--)
                {
                    if (this.commandExecutor.ApplicationState.Packages.AllPackages.Any(p => p.PackageFullName == selected[i]))
                    {
                        continue;
                    }

                    selected.RemoveAt(i);
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