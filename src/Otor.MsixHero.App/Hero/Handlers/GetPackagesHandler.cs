using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetPackagesHandler : IRequestHandler<GetPackagesCommand, IList<PackageEntry>>, IObserver<ActivePackageFullNames>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IBusyManager _busyManager;
        private readonly IUacElevation _uacElevation;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRunningAppsDetector _detector;

        public GetPackagesHandler(
            IMsixHeroCommandExecutor commandExecutor,
            IBusyManager busyManager,
            IUacElevation uacElevation,
            IEventAggregator eventAggregator,
            IRunningAppsDetector detector)
        {
            detector.Subscribe(this);
            this._commandExecutor = commandExecutor;
            this._busyManager = busyManager;
            this._uacElevation = uacElevation;
            this._eventAggregator = eventAggregator;
            this._detector = detector;
        }

        public async Task<IList<PackageEntry>> Handle(GetPackagesCommand request, CancellationToken cancellationToken)
        {
            var context = this._busyManager.Begin(OperationType.PackageLoading);
            try
            {
                PackageQuerySource mode;
                var selected = this._commandExecutor.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).ToList();

                List<PackageEntry> results;
                if (request.Source.HasValue && request.Source.Value.Type == PackageQuerySourceType.Directory)
                {
                    SearchOption so;
                    var actualPath = request.Source.Value.Path;
                    if (actualPath.EndsWith("/*", StringComparison.OrdinalIgnoreCase) || actualPath.EndsWith("\\*", StringComparison.OrdinalIgnoreCase))
                    {
                        so = SearchOption.AllDirectories;
                        actualPath = actualPath[..^2];
                    }
                    else
                    {
                        so = SearchOption.TopDirectoryOnly;
                    }

                    var allFiles = System.IO.Directory.EnumerateFiles(actualPath, "*.msix", so);

                    results = await PackageEntryExtensions.FromReaders(allFiles.Select(FileReaderFactory.CreateFileReader), checkIfRunning: true, cancellationToken: cancellationToken).ToListAsync(cancellationToken: cancellationToken);

                    this._commandExecutor.ApplicationState.Packages.AllPackages.Clear();
                    this._commandExecutor.ApplicationState.Packages.AllPackages.AddRange(results);

                    mode = PackageQuerySource.FromFolder(actualPath, so == SearchOption.AllDirectories);
                }
                else
                {
                    var manager = request.Source.HasValue && request.Source.Value.Type == PackageQuerySourceType.InstalledForAllUsers ? this._uacElevation.AsAdministrator<IAppxPackageQueryService>() : this._uacElevation.AsHighestAvailable<IAppxPackageQueryService>();

                    if (request.Source.HasValue)
                    {
                        mode = request.Source.Value;
                        if (mode.Type == PackageQuerySourceType.Installed)
                        {
                            var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken);
                            mode = isAdmin ? PackageQuerySource.InstalledForAllUsers() : PackageQuerySource.InstalledForCurrentUser();
                        }
                    }
                    else
                    {
                        mode = this._commandExecutor.ApplicationState.Packages.Mode;
                    }

                    results = await manager.GetInstalledPackages(mode.Type, cancellationToken, context).ConfigureAwait(false);

                    var currentlyRunning = new HashSet<string>(this._detector.GetCurrentlyRunningPackageFamilyNames(), StringComparer.Ordinal);

                    foreach (var item in this._commandExecutor.ApplicationState.Packages.AllPackages)
                    {
                        item.IsRunning = currentlyRunning.Contains(item.PackageFamilyName);
                    }

                    // this.packageListSynchronizer.EnterWriteLock();
                    this._commandExecutor.ApplicationState.Packages.AllPackages.Clear();
                    this._commandExecutor.ApplicationState.Packages.AllPackages.AddRange(results);
                }

                this._commandExecutor.ApplicationState.Packages.Mode = mode;

                // Just in case, make sure to remove stuff from the selection if the list of packages does not have it anymore.
                for (var i = selected.Count - 1; i >= 0; i--)
                {
                    if (this._commandExecutor.ApplicationState.Packages.AllPackages.Any(p => p.PackageFullName == selected[i]))
                    {
                        continue;
                    }

                    selected.RemoveAt(i);
                }

                await this._commandExecutor.Invoke(this, new SelectPackagesCommand(selected), cancellationToken).ConfigureAwait(false);

                return results;
            }
            finally
            {
                this._busyManager.End(context);
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
            this._commandExecutor.ApplicationState.Packages.ActivePackageNames = value.Running;
            this._eventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Publish(value);
        }
    }
}