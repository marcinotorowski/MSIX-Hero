using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.SystemState.Services;
using otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty;
using otor.msixhero.lib.Domain.SystemState;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel
{
    public class SystemStatusViewModel : NotifyPropertyChanged, IHeaderViewModel, INavigationAware, IActiveAware
    {
        private readonly IThirdPartyDetector thirdPartyDetector;
        private readonly IServiceRecommendationAdvisor serviceAdvisor;
        private bool isActive;

        public SystemStatusViewModel(
            IThirdPartyDetector thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor)
        {
            this.thirdPartyDetector = thirdPartyDetector;
            this.serviceAdvisor = serviceAdvisor;
            this.DiscoveredApps = new AsyncProperty<ObservableCollection<DiscoveredAppViewModel>>();
            this.ServiceRecommendations = new AsyncProperty<ObservableCollection<ServiceRecommendationViewModel>>();
        }

        public bool ServicesRequireAttention { get; private set; }

        public string DiscoveredAppsCaption { get; private set; }

        public AsyncProperty<ObservableCollection<ServiceRecommendationViewModel>> ServiceRecommendations { get; set; }

        public AsyncProperty<ObservableCollection<DiscoveredAppViewModel>> DiscoveredApps { get; }

        public string Header { get; } = "System status";

        public Geometry Icon { get; } = Geometry.Parse("M 2 6 L 2 24 L 15 24 L 15 26 L 10 26 L 10 28 L 22 28 L 22 26 L 17 26 L 17 24 L 30 24 L 30 6 Z M 4 8 L 28 8 L 28 22 L 4 22 Z M 13.125 8.5 L 10.375 14 L 5 14 L 5 16 L 11.625 16 L 12.875 13.5 L 15.0625 19.34375 L 15.71875 21.0625 L 16.8125 19.59375 L 19.5 16 L 24 16 L 24 14 L 18.5 14 L 18.1875 14.40625 L 16.28125 16.9375 L 13.9375 10.65625 Z");

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
                    this.Refresh().ContinueWith(t =>
                    {
                        this.ServicesRequireAttention = this.ServiceRecommendations.CurrentValue.Any(c => c.IsRunning != c.ShouldRun);
                        this.OnPropertyChanged(nameof(ServicesRequireAttention));

                        var items = this.DiscoveredApps.CurrentValue.Count;
                        switch (items)
                        {
                            case 0:
                                this.DiscoveredAppsCaption = "No discovered app found.";
                                break;
                            case 1:
                                this.DiscoveredAppsCaption = "One discovered app found.";
                                break;
                            default:
                                this.DiscoveredAppsCaption = $"{items} discovered apps found.";
                                break;
                        }

                        this.OnPropertyChanged(nameof(DiscoveredAppsCaption));
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent,
                    TaskScheduler.FromCurrentSynchronizationContext());
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
            this.Refresh();
        }

        private Task Refresh()
        {
            return Task.WhenAll(
                this.DiscoveredApps.Load(this.LoadDiscoveredApps()), 
                this.ServiceRecommendations.Load(this.LoadServices()));
        }

        public async Task<ObservableCollection<DiscoveredAppViewModel>> LoadDiscoveredApps()
        {
            var list = await Task.Run(() => this.thirdPartyDetector.DetectApps().ToList()).ConfigureAwait(true);
            return new ObservableCollection<DiscoveredAppViewModel>(list.Select(item => new DiscoveredAppViewModel(item)));
        }

        public async Task<ObservableCollection<ServiceRecommendationViewModel>> LoadServices()
        {
            var list = await Task.Run(() => this.serviceAdvisor.Advise(AdvisorMode.ForPackaging).ToList()).ConfigureAwait(true);
            return new ObservableCollection<ServiceRecommendationViewModel>(list.Select(item => new ServiceRecommendationViewModel(item)));
        }
    }
}
