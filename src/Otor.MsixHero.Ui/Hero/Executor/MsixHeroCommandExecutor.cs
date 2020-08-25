using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Executor
{
    public class MsixHeroCommandExecutor : BaseMsixHeroCommandExecutor
    {
        private readonly IConfigurationService configurationService;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider;

        public MsixHeroCommandExecutor(
            IEventAggregator eventAggregator,
            IConfigurationService configurationService,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider) : base(eventAggregator)
        {
            this.configurationService = configurationService;
            this.packageManagerProvider = packageManagerProvider;
            this.volumeManagerProvider = volumeManagerProvider;
            this.Handlers[typeof(GetVolumesCommand)] = (command, token, progress) => this.GetVolumes((GetVolumesCommand)command, token, progress);
            this.Handlers[typeof(SelectVolumesCommand)] = (command, token, progress) => this.SelectVolumes((SelectVolumesCommand)command);
            this.Handlers[typeof(SetVolumeFilterCommand)] = (command, token, progress) => this.SetVolumeFilter((SetVolumeFilterCommand)command);

            this.Handlers[typeof(GetPackagesCommand)] = (command, token, progress) => this.GetPackages((GetPackagesCommand)command, token, progress);
            this.Handlers[typeof(SelectPackagesCommand)] = (command, token, progress) => this.SelectPackages((SelectPackagesCommand)command);

            this.Handlers[typeof(SetPackageFilterCommand)] = (command, token, progress) => this.SetPackageFilter((SetPackageFilterCommand)command);
            this.Handlers[typeof(SetCurrentModeCommand)] = (command, token, progress) => this.SetCurrentMode((SetCurrentModeCommand)command);
            this.Handlers[typeof(SetPackageSortingCommand)] = (command, token, progress) => this.SetPackageSorting((SetPackageSortingCommand)command);
            this.Handlers[typeof(SetPackageGroupingCommand)] = (command, token, progress) => this.SetPackageGrouping((SetPackageGroupingCommand)command);
            this.Handlers[typeof(SetPackageSidebarVisibilityCommand)] = (command, token, progress) => this.SetPackageSidebarVisibility((SetPackageSidebarVisibilityCommand)command);
        }

        private async Task<PackageGroup> SetPackageGrouping(SetPackageGroupingCommand command)
        {
            this.ApplicationState.Packages.GroupMode = command.GroupMode;
            
            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);
            cleanConfig.List ??= new ListConfiguration();
            cleanConfig.List.Group ??= new GroupConfiguration();
            cleanConfig.List.Group.GroupMode = this.ApplicationState.Packages.GroupMode;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig).ConfigureAwait(false);
            return this.ApplicationState.Packages.GroupMode;

        }

        private async Task SetPackageSidebarVisibility(SetPackageSidebarVisibilityCommand command)
        {
            this.ApplicationState.Packages.ShowSidebar = command.IsVisible;
            
            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);
            cleanConfig.List ??= new ListConfiguration();
            cleanConfig.List.Sidebar ??= new SidebarListConfiguration();
            cleanConfig.List.Sidebar.Visible = this.ApplicationState.Packages.ShowSidebar;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig).ConfigureAwait(false);
        }

        private async Task SetPackageSorting(SetPackageSortingCommand command)
        {
            this.ApplicationState.Packages.SortMode = command.SortMode;

            if (command.Descending.HasValue)
            {
                this.ApplicationState.Packages.SortDescending = command.Descending.Value;
            }
            else
            {
                this.ApplicationState.Packages.SortDescending = !this.ApplicationState.Packages.SortDescending;
            }

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);
            cleanConfig.List ??= new ListConfiguration();
            cleanConfig.List.Sorting ??= new SortConfiguration();
            cleanConfig.List.Sorting.SortingMode = this.ApplicationState.Packages.SortMode;
            cleanConfig.List.Sorting.Descending = this.ApplicationState.Packages.SortDescending;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig).ConfigureAwait(false);
        }

        private Task SetCurrentMode(SetCurrentModeCommand command)
        {
            this.ApplicationState.CurrentMode = command.NewMode;
            return Task.FromResult(true);
        }

        private async Task SetPackageFilter(SetPackageFilterCommand command)
        {
            this.ApplicationState.Packages.PackageFilter = command.PackageFilter;
            this.ApplicationState.Packages.AddonFilter = command.AddonFilter;
            this.ApplicationState.Packages.SearchKey = command.SearchKey;

            var cleanConfig = await this.configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);
            cleanConfig.List ??= new ListConfiguration();
            cleanConfig.List.Filter ??= new FilterConfiguration();
            cleanConfig.List.Filter.ShowApps = command.PackageFilter;
            cleanConfig.List.Filter.AddonsFilter = command.AddonFilter;
            await this.configurationService.SetCurrentConfigurationAsync(cleanConfig).ConfigureAwait(false);
        }

        private Task SetVolumeFilter(SetVolumeFilterCommand command)
        {
            this.ApplicationState.Volumes.SearchKey = command.SearchKey;
            return Task.FromResult(true);
        }

        private Task<IList<AppxVolume>> SelectVolumes(SelectVolumesCommand command)
        {
            IList<AppxVolume> selected;
            if (!command.SelectedVolumePaths.Any())
            {
                selected = new List<AppxVolume>();
            }
            else if (command.SelectedVolumePaths.Count == 1)
            {
                var singleSelection = this.ApplicationState.Volumes.AllVolumes.FirstOrDefault(a => string.Equals(a.PackageStorePath, command.SelectedVolumePaths[0], StringComparison.OrdinalIgnoreCase));
                if (singleSelection != null)
                {
                    selected = new List<AppxVolume> { singleSelection };
                }
                else
                {
                    selected = new List<AppxVolume>();
                }
            }
            else
            {
                selected = this.ApplicationState.Volumes.AllVolumes.Where(a => command.SelectedVolumePaths.Contains(a.PackageStorePath)).ToList();
            }

            this.ApplicationState.Volumes.SelectedVolumes.Clear();
            this.ApplicationState.Volumes.SelectedVolumes.AddRange(selected);
            return Task.FromResult(selected);
        }

        private Task<IList<InstalledPackage>> SelectPackages(SelectPackagesCommand command)
        {
            IList<InstalledPackage> selected;
            if (!command.SelectedManifestPaths.Any())
            {
                selected = new List<InstalledPackage>();
            }
            else if (command.SelectedManifestPaths.Count == 1)
            {
                var singleSelection = this.ApplicationState.Packages.AllPackages.FirstOrDefault(a => string.Equals(a.ManifestLocation, command.SelectedManifestPaths[0], StringComparison.OrdinalIgnoreCase));
                if (singleSelection != null)
                {
                    selected = new List<InstalledPackage> { singleSelection };
                }
                else
                {
                    selected = new List<InstalledPackage>();
                }
            }
            else
            {
                selected = this.ApplicationState.Packages.AllPackages.Where(a => command.SelectedManifestPaths.Contains(a.ManifestLocation)).ToList();
            }

            this.ApplicationState.Packages.SelectedPackages.Clear();
            this.ApplicationState.Packages.SelectedPackages.AddRange(selected);
            return Task.FromResult(selected);
        }

        private async Task<IList<InstalledPackage>> GetPackages(GetPackagesCommand command, CancellationToken cancellationToken, IProgress<ProgressData> progressData)
        {
            SelfElevationLevel level = SelfElevationLevel.HighestAvailable;

            if (command.FindMode == PackageFindMode.AllUsers)
            {
                level = SelfElevationLevel.AsAdministrator;
            }

            var manager = await this.packageManagerProvider.GetProxyFor(level, cancellationToken).ConfigureAwait(false);

            PackageFindMode mode;
            if (command.FindMode.HasValue)
            {
                mode = command.FindMode.Value;
                if (mode == PackageFindMode.Auto)
                {
                    var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken);
                    mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
                }
            }
            else
            {
                switch (this.ApplicationState.Packages.Mode)
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

            var selected = this.ApplicationState.Packages.SelectedPackages.Select(p => p.ManifestLocation).ToArray();

            var results = await manager.GetInstalledPackages(mode, cancellationToken, progressData).ConfigureAwait(false);

            this.ApplicationState.Packages.AllPackages.Clear();
            this.ApplicationState.Packages.AllPackages.AddRange(results);

            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    this.ApplicationState.Packages.Mode = PackageContext.CurrentUser;
                    break;
                case PackageFindMode.AllUsers:
                    this.ApplicationState.Packages.Mode = PackageContext.AllUsers;
                    break;
            }

            // await this.Invoke(this, new SelectPackagesCommand(), cancellationToken).ConfigureAwait(false);
            if (selected.Any())
            {
                await this.Invoke(this, new SelectPackagesCommand(selected), cancellationToken).ConfigureAwait(false);
            }

            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    this.ApplicationState.Packages.Mode = PackageContext.CurrentUser;
                    break;
                case PackageFindMode.AllUsers:
                    this.ApplicationState.Packages.Mode = PackageContext.AllUsers;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return results;
        }

        // ReSharper disable once UnusedParameter.Local
        private async Task<IList<AppxVolume>> GetVolumes(GetVolumesCommand command, CancellationToken cancellationToken, IProgress<ProgressData> progressData)
        {
            var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            
            var selected = this.ApplicationState.Volumes.SelectedVolumes.Select(p => p.PackageStorePath).ToArray();

            var results = await manager.GetAll(cancellationToken, progressData).ConfigureAwait(false);
            var defaultVol = await manager.GetDefault(cancellationToken).ConfigureAwait(false);

            var findMe = defaultVol == null ? null : results.FirstOrDefault(d => d.PackageStorePath == defaultVol.PackageStorePath);
            if (findMe != null)
            {
                findMe.IsDefault = true;
            }

            this.ApplicationState.Volumes.AllVolumes.Clear();
            this.ApplicationState.Volumes.AllVolumes.AddRange(results);

            if (selected.Any())
            {
                await this.Invoke(this, new SelectVolumesCommand(selected), cancellationToken).ConfigureAwait(false);
            }
            
            return results;
        }
    }
}