using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Commands;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class SinglePackageViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IApplicationStateManager stateManager;
        private string currentPackageId;
        private InstalledPackageViewModel selectedInstalledPackage;
        private ICommand findUsers;
        private string error;

        public SinglePackageViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

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

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            this.Error = null;
            var param = navigationContext.Parameters["Packages"] as IList<string>;
            if (param == null || param.Count != 1)
            {
                return;
            }

            this.currentPackageId = param.FirstOrDefault();

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
            
            this.SelectedInstalledPackage = lastSelected == null ? null : new InstalledPackageViewModel(lastSelected);

#pragma warning disable 4014
            if (lastSelected == null || this.HasError)
            {
                this.SelectedPackageUsersInfo.Load(Task.FromResult((FoundUsersViewModel)null));
                this.SelectedPackageManifestInfo.Load(Task.FromResult((InstalledPackageDetailsViewModel)null));
            }
            else
            {
                this.SelectedPackageManifestInfo.Load(this.GetManifestDetails(lastSelected));

                try
                {
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.IsSelfElevated));
                }
                catch (Exception)
                {
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, false));
                }
#pragma warning restore 4014   
            }
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var param = navigationContext.Parameters["Packages"] as IList<string>;
            if (param == null || param.Count != 1)
            {
                return false;
            }

            return true;
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
            catch (Exception e)
            {
                return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
            }
        }

        private async Task<InstalledPackageDetailsViewModel> GetManifestDetails(InstalledPackage package)
        {
            var cmd = new GetPackageDetails(package);
            var manifestDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(cmd).ConfigureAwait(false);
            if (manifestDetails == null)
            {
                return null;
            }

            return new InstalledPackageDetailsViewModel(manifestDetails);
        }

    }
}
