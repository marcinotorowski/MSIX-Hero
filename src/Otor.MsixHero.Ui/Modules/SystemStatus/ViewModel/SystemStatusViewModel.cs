using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.State;
using Otor.MsixHero.Ui.Modules.Common;
using Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.DeveloperMode;
using Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.Repackaging;
using Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.Tooling;
using Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using Otor.MsixHero.Ui.Themes;
using Otor.MsixHero.Ui.ViewModel;
using Prism;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel
{
    public class SystemStatusViewModel : NotifyPropertyChanged, IHeaderViewModel, INavigationAware, IActiveAware
    {
        protected readonly ISideloadingChecker SideLoadCheck = new RegistrySideloadingChecker();
        private readonly IMsixHeroApplication application;
        private bool isActive;
        private bool isLoading;

        public SystemStatusViewModel(
            IMsixHeroApplication application,
            IThirdPartyAppProvider thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService)
        {
            this.application = application;
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

                this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.SystemStatus));
                this.Refresh();
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
