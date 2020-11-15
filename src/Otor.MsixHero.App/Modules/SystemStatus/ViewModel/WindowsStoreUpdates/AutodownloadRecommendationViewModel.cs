using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates
{
    public class AutoDownloadRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly ISideloadingChecker SideloadingChecker;
        private WindowsStoreAutoDownload autoDownloadStatus;

        public AutoDownloadRecommendationViewModel(ISideloadingChecker sideloadingChecker)
        {
            this.SideloadingChecker = sideloadingChecker;
            this.SetStatusAndSummary();
        }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 16 4 C 12.426807 4 9.1889768 5.5940841 7 8.1113281 L 7 5 L 5 5 L 5 12 L 12 12 L 12 10 L 8.0546875 10 C 9.8580114 7.5851367 12.758494 6 16 6 C 20.288652 6 23.809592 8.6271915 25.267578 12.363281 L 27.130859 11.636719 C 25.388846 7.1728084 21.111348 4 16 4 z M 6.7324219 19.636719 L 4.8691406 20.363281 C 6.6111543 24.827191 10.888652 28 16 28 C 19.599779 28 22.811088 26.3919 25 23.916016 L 25 27 L 27 27 L 27 20 L 20 20 L 20 22 L 23.96875 22 C 22.155848 24.387604 19.278082 26 16 26 C 11.711348 26 8.1904082 23.372809 6.7324219 19.636719 z");
        }

        private void SetStatusAndSummary()
        {
            this.autoDownloadStatus = this.SideloadingChecker.GetStoreAutoDownloadStatus();

            switch (this.AutoDownloadStatus)
            {
                case WindowsStoreAutoDownload.Default:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "Default policy applies.";
                    break;
                case WindowsStoreAutoDownload.Always:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "Automatic download is enabled.";
                    break;
                case WindowsStoreAutoDownload.Never:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = "Automatic download is disabled.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public WindowsStoreAutoDownload AutoDownloadStatus
        {
            get => this.autoDownloadStatus;
            set
            {
                if (this.autoDownloadStatus == value)
                {
                    return;
                }

                if (!this.SideloadingChecker.SetStoreAutoDownloadStatus(value))
                {
                    return;
                }

                this.autoDownloadStatus = value;
                this.OnPropertyChanged();
                this.SetStatusAndSummary();
            }
        }
        
        public override string Title { get; } = "Automatic updates of Store apps";

        public override Task Refresh(CancellationToken cancellationToken = default)
        {
            this.SetStatusAndSummary();
            this.OnPropertyChanged(nameof(this.AutoDownloadStatus));
            return Task.FromResult(true);
        }
    }
}