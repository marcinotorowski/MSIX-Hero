using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private readonly IInteractionService interactionService;
        private FileInfo currentManifest;
        private ICommand findUsers;
        private string error;

        public PackageContentViewModel(IApplicationStateManager stateManager, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            this.CommandHandler = new PackageContentCommandHandler(configurationService, interactionService);
        }

        public PackageContentCommandHandler CommandHandler { get; }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();

        public AsyncProperty<PackageContentDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<PackageContentDetailsViewModel>();

        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ?? (this.findUsers = new DelegateCommand(() =>
                {
                    if (this.currentManifest == null)
                    {
                        return;
                    }

#pragma warning disable 4014
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(true));
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

            var manifest = navigation.SelectedManifests.First();
            if (this.currentManifest != null && string.Equals(this.currentManifest.FullName, manifest, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (manifest == null)
            {
                this.Error = "Could not find package manifest.";
            }
            else
            {
                this.currentManifest = new FileInfo(manifest);
                if (!currentManifest.Exists)
                {
                    this.Error = "Could not load the manifest";
                }
                else
                {
                    this.Error = null;
                }
            }

#pragma warning disable 4014
            if (this.HasError)
            {
                this.SelectedPackageUsersInfo.Load(Task.FromResult((FoundUsersViewModel)null));
                this.SelectedPackageManifestInfo.Load(Task.FromResult((PackageContentDetailsViewModel)null));
            }
            else
            {
                await this.SelectedPackageManifestInfo.Load(this.GetManifestDetails()).ConfigureAwait(false);

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

        private async Task<FoundUsersViewModel> GetSelectionDetails(bool forceElevation)
        {
            if (this.currentManifest == null || !this.currentManifest.Exists)
            {
                return null;
            }

            try
            {
                var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new FindUsers(this.currentManifest.FullName, forceElevation)).ConfigureAwait(false);
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

        private async Task<PackageContentDetailsViewModel> GetManifestDetails()
        {
            if (this.currentManifest == null || !this.currentManifest.Exists)
            {
                return null;
            }

            try
            {
                var cmd = new GetPackageDetails(this.currentManifest.FullName);
                var manifestDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(cmd).ConfigureAwait(false);
                if (manifestDetails == null)
                {
                    return null;
                }

                return new PackageContentDetailsViewModel(manifestDetails);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e, InteractionResult.OK);
                return null;
            }
        }
    }
}
