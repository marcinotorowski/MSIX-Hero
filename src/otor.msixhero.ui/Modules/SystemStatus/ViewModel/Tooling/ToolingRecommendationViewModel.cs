using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;

namespace Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.Tooling
{
    public class ToolingRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly IThirdPartyAppProvider ThirdPartyDetector;

        public ToolingRecommendationViewModel(IThirdPartyAppProvider thirdPartyDetector)
        {
            this.ThirdPartyDetector = thirdPartyDetector;
            this.Items = new ObservableCollection<DiscoveredAppViewModel>();
        }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 4 4 L 4 28.03125 L 28.03125 28.03125 L 28.03125 22 L 23 22 L 23 4 Z M 6 6 L 15 6 L 15 22 L 6 22 Z M 17 6 L 21 6 L 21 22 L 17 22 Z M 8 8 L 8 10 L 13 10 L 13 8 Z M 8 12 L 8 14 L 13 14 L 13 12 Z M 17.875 19 L 17.875 21 L 20 21 L 20 19 Z M 6 24 L 26.03125 24 L 26.03125 26.03125 L 6 26.03125 Z");
        }
        
        public override async Task Refresh(CancellationToken cancellationToken = default)
        {
            var thirdParty = await Task.Run(() => this.ThirdPartyDetector.ProvideApps().ToList(), cancellationToken).ConfigureAwait(false);
            
            this.Items = new ObservableCollection<DiscoveredAppViewModel>();
            foreach (var app in thirdParty)
            {
                if (app is IThirdPartyDetectedApp)
                {
                    this.Items.Add(new DiscoveredAppViewModel(app, DiscoveredAppViewModelStatus.Installed));
                }
                else if (app is IStoreApp)
                {
                    this.Items.Add(new DiscoveredAppViewModel(app, DiscoveredAppViewModelStatus.Available));
                }
                else
                {
                    this.Items.Add(new DiscoveredAppViewModel(app));
                }
            }

            this.OnPropertyChanged(nameof(this.Items));

            switch (this.Items.Count(item => item.Status == DiscoveredAppViewModelStatus.Installed))
            {
                case 0:
                    this.Summary = "No MSIX authoring tool detected on this machine.";
                    this.Status = RecommendationStatus.Warning;
                    break;
                case 1:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "One MSIX authoring tool has been discovered.";
                    break;
                case 2:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "Two MSIX authoring tools have been discovered.";
                    break;
                case 3:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "Three MSIX authoring tools have been discovered.";
                    break;
                default:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = $"{this.Items.Count} MSIX authoring tools have been discovered.";
                    break;
            }
        }

        public ObservableCollection<DiscoveredAppViewModel> Items { get; private set; }

        public override string Title { get; } = "MSIX Tooling";
    }
}
