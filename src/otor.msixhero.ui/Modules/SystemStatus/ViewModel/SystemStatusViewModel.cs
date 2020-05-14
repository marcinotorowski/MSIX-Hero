using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.SystemState.Services;
using otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.DeveloperMode;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.Repackaging;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.Tooling;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using otor.msixhero.ui.Themes;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel
{
    public class SystemStatusViewModel : NotifyPropertyChanged, IHeaderViewModel, INavigationAware, IActiveAware
    {
        protected readonly ISideloadingChecker SideLoadCheck = new RegistrySideloadingChecker();
        private readonly IApplicationStateManager stateManager;
        private bool isActive;
        private bool isLoading;

        public SystemStatusViewModel(
            IApplicationStateManager stateManager,
            IThirdPartyAppProvider thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService)
        {
            this.stateManager = stateManager;
            this.Items = new ObservableCollection<BaseRecommendationViewModel>();

            var sideloading = new DeveloperAndSideloadingRecommendationViewModel(this.SideLoadCheck);
            var storeAutoDownload = new AutoDownloadRecommendationViewModel(this.SideLoadCheck);
            var repackaging = new RepackagingRecommendationViewModel(serviceAdvisor, interactionService, storeAutoDownload);
            var tooling = new ToolingRecommendationViewModel(thirdPartyDetector);

            this.Items.Add(sideloading);
            this.Items.Add(storeAutoDownload);
            this.Items.Add(repackaging);
            this.Items.Add(tooling);
        }

        public SystemStatusCommandHandler CommandHandler { get; } = new SystemStatusCommandHandler();

        public ObservableCollection<BaseRecommendationViewModel> Items { get; }

        public string Header { get; } = "System status";

        public Geometry Icon { get; } = VectorIcons.TabSystemStatus;

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.IsActiveChanged?.Invoke(this, new EventArgs());

                if (value)
                {
                    try
                    {
                        this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.SystemStatus));
                    }
                    catch (UserHandledException)
                    {
                        return;
                    }

                    this.Refresh();
                }
            }
        }

        public event EventHandler IsActiveChanged;

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public void Refresh()
        {
#pragma warning disable 4014
            this.IsLoading = true;

            var allTasks = this.Items.Select(t => t.Refresh());
            Task.WhenAll(allTasks).ContinueWith(t => { this.IsLoading = false; });
#pragma warning restore 4014
        }
    }
}
