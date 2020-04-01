using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements;
using otor.msixhero.ui.Modules.PackageList.Navigation;
using otor.msixhero.ui.ViewModel;
using Prism.Commands;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel
{
    public class PackageContentViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IApplicationStateManager stateManager;
        private ICommand findUsers;
        private string fullPackageName;

        public PackageContentViewModel(IApplicationStateManager stateManager, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.stateManager = stateManager;
            this.CommandHandler = new PackageContentCommandHandler(configurationService, interactionService);
        }

        public PackageContentCommandHandler CommandHandler { get; }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();

        public AsyncProperty<PackageContentDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<PackageContentDetailsViewModel>();

        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ??= new DelegateCommand(
                    () =>
                    {
                        if (this.fullPackageName == null)
                        {
                            return;
                        }

#pragma warning disable 4014
                        this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(true));
#pragma warning restore 4014
                    });
            }
        }
        
        public async Task<PackageContentDetailsViewModel> LoadPackage(IAppxFileReader source, CancellationToken cancellationToken = default)
        {
            if (!source.FileExists("AppxManifest.xml"))
            {
                throw new FileNotFoundException("Could not find AppxManifest.xml");
            }

            var appxReader = new AppxManifestReader();
            var appxManifest = await appxReader.Read(source, cancellationToken).ConfigureAwait(false);

            this.fullPackageName = appxManifest.FullName;
            if (!this.stateManager.CurrentState.IsElevated && !this.stateManager.CurrentState.IsSelfElevated)
            {
                await this.SelectedPackageUsersInfo.Load(Task.FromResult(new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired))).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.IsSelfElevated)).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(false)).ConfigureAwait(false);
                }
            }

            return new PackageContentDetailsViewModel(appxManifest);
        }

        async void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            if (navigation.SelectedManifests?.Count != 1)
            {
                return;
            }

            var manifest = navigation.SelectedManifests.First();
            using IAppxFileReader fileReader = new FileInfoFileReaderAdapter(manifest);
            await this.SelectedPackageManifestInfo.Load(this.LoadPackage(fileReader, CancellationToken.None)).ConfigureAwait(false);
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            return navigation.SelectedManifests?.Count == 1;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private async Task<FoundUsersViewModel> GetSelectionDetails(bool forceElevation)
        {
            try
            {
                var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new FindUsers(this.fullPackageName, forceElevation)).ConfigureAwait(false);
                if (stateDetails == null)
                {
                    return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
                }

                return new FoundUsersViewModel(stateDetails, ElevationStatus.OK);
            }
            catch (Exception)
            {
                return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
            }
        }
    }
}
