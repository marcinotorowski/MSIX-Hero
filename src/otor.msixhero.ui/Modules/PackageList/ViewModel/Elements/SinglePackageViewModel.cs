using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.PackageList.Navigation;
using otor.msixhero.ui.ViewModel;
using Prism.Commands;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class SinglePackageViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IInteractionService interactionService;
        private string currentManifest;
        private InstalledPackageViewModel selectedInstalledPackage;
        private ICommand findUsers;
        private string error;

        public SinglePackageViewModel(IApplicationStateManager stateManager, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            this.CommandHandler = new SinglePackageCommandHandler(configurationService, interactionService);
        }

        public SinglePackageCommandHandler CommandHandler { get; }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();

        public AsyncProperty<InstalledPackageDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<InstalledPackageDetailsViewModel>();

        public InstalledPackageViewModel SelectedInstalledPackage
        {
            get => this.selectedInstalledPackage;
            set => this.SetField(ref this.selectedInstalledPackage, value);
        }
        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ?? (this.findUsers = new DelegateCommand(() =>
                {
                    if (this.SelectedInstalledPackage == null)
                    {
                        return;
                    }

#pragma warning disable 4014
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(this.SelectedInstalledPackage.Model, true));
#pragma warning restore 4014
                }));
            }
        }

        public bool HasError => this.error != null;

        public string Error
        {
            get => this.error;
            private set 
            {
                if (this.SetField(ref this.error, value))
                {
                    this.OnPropertyChanged(nameof(HasError));
                }
            }
        }

        async void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            this.Error = null;

            var navigation = new PackageListNavigation(navigationContext);
            if (navigation.SelectedManifests?.Count != 1)
            {
                return;
            }

            this.currentManifest = navigation.SelectedManifests.First();
            var lastSelected = this.stateManager.CurrentState.Packages.SelectedItems.LastOrDefault();
            if (lastSelected?.InstallLocation == null)
            {
                this.Error = "Could not load the manifest";
                this.SelectedInstalledPackage = null;
            }
            else
            {
                this.Error = null;
            }
            
            this.SelectedInstalledPackage = lastSelected == null ? null : new InstalledPackageViewModel(lastSelected, this.stateManager);

#pragma warning disable 4014
            if (lastSelected == null || this.HasError)
            {
                this.SelectedPackageUsersInfo.Load(Task.FromResult((FoundUsersViewModel)null));
                this.SelectedPackageManifestInfo.Load(Task.FromResult((InstalledPackageDetailsViewModel)null));
            }
            else
            {
                await this.SelectedPackageManifestInfo.Load(this.GetManifestDetails(lastSelected)).ConfigureAwait(false);

                if (!this.stateManager.CurrentState.IsElevated && !this.stateManager.CurrentState.IsSelfElevated)
                {
                    await this.SelectedPackageUsersInfo.Load(Task.FromResult(new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired))).ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.IsSelfElevated)).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, false)).ConfigureAwait(false);
                    }
                }
#pragma warning restore 4014   
            }
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            return navigation.SelectedManifests?.Count == 1;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private async Task<FoundUsersViewModel> GetSelectionDetails(InstalledPackage package, bool forceElevation)
        {
            try
            {
                var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new FindUsers(package, forceElevation)).ConfigureAwait(false);
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

        private async Task<InstalledPackageDetailsViewModel> GetManifestDetails(InstalledPackage package)
        {
            try
            {
                var cmd = new GetPackageDetails(package);
                var manifestDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(cmd).ConfigureAwait(false);
                if (manifestDetails == null)
                {
                    return null;
                }

                return new InstalledPackageDetailsViewModel(manifestDetails);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e, InteractionResult.OK);
                return null;
            }
        }
    }
}
