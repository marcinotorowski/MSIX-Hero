using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.SystemState.Services;
using otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.DeveloperMode;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.Repackaging;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.Tooling;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
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
            IThirdPartyDetector thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService)
        {
            this.stateManager = stateManager;
            this.Items = new ObservableCollection<BaseRecommendationViewModel>();

            var item4 = new ToolingRecommendationViewModel(thirdPartyDetector);
            var item3 = new AutoDownloadRecommendationViewModel(this.SideLoadCheck);
            var item2 = new RepackagingRecommendationViewModel(serviceAdvisor, interactionService, item3);
            var item1 = new DeveloperAndSideloadingRecommendationViewModel(this.SideLoadCheck);

            this.Items.Add(item1);
            this.Items.Add(item2);
            this.Items.Add(item3);
            this.Items.Add(item4);
        }

        public SystemStatusCommandHandler CommandHandler { get; } = new SystemStatusCommandHandler();

        public ObservableCollection<BaseRecommendationViewModel> Items { get; }

        public string Header { get; } = "System status";

        public Geometry Icon { get; } = Geometry.Parse("M 2 6 L 2 24 L 15 24 L 15 26 L 10 26 L 10 28 L 22 28 L 22 26 L 17 26 L 17 24 L 30 24 L 30 6 Z M 4 8 L 28 8 L 28 22 L 4 22 Z M 13.125 8.5 L 10.375 14 L 5 14 L 5 16 L 11.625 16 L 12.875 13.5 L 15.0625 19.34375 L 15.71875 21.0625 L 16.8125 19.59375 L 19.5 16 L 24 16 L 24 14 L 18.5 14 L 18.1875 14.40625 L 16.28125 16.9375 L 13.9375 10.65625 Z");

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
                    this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.SystemStatus));
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
