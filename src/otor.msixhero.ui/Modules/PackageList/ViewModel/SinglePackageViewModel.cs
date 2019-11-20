using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
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
        private PackageViewModel selectedPackage;
        private ICommand findUsers;

        public SinglePackageViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();

        public AsyncProperty<AppxDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<AppxDetailsViewModel>();

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var param = navigationContext.Parameters[nameof(Package.ProductId)] as string;
            this.currentPackageId = param;

            var lastSelected = this.stateManager.CurrentState.Packages.SelectedItems.LastOrDefault();

            this.SelectedPackage = lastSelected == null ? null : new PackageViewModel(lastSelected);

#pragma warning disable 4014
            if (lastSelected == null)
            {
                this.SelectedPackageUsersInfo.Load(Task.FromResult((FoundUsersViewModel)null));
                this.SelectedPackageManifestInfo.Load(Task.FromResult((AppxDetailsViewModel)null));
            }
            else
            {
                this.SelectedPackageManifestInfo.Load(this.GetManifestDetails(lastSelected));

                try
                {
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.HasSelfElevated));
                }
                catch (Exception)
                {
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(lastSelected, false));
                }
#pragma warning restore 4014   
            }
        }

        public PackageViewModel SelectedPackage
        {
            get => this.selectedPackage;
            set => this.SetField(ref this.selectedPackage, value);
        }
        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ?? (this.findUsers = new DelegateCommand(() =>
                {
                    if (this.SelectedPackage == null)
                    {
                        return;
                    }

#pragma warning disable 4014
                    this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(this.SelectedPackage.Model, true));
#pragma warning restore 4014
                }));
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            var param = navigationContext.Parameters[nameof(Package.ProductId)] as string;
            return param == this.currentPackageId;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private async Task<FoundUsersViewModel> GetSelectionDetails(Package package, bool forceElevation)
        {
            var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new FindUsers(package, forceElevation)).ConfigureAwait(false);
            return new FoundUsersViewModel(stateDetails);
        }

        private async Task<AppxDetailsViewModel> GetManifestDetails(Package package)
        {
            var manifestDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackageDetails(package)).ConfigureAwait(false);
            return new AppxDetailsViewModel(manifestDetails);
        }

    }
}
