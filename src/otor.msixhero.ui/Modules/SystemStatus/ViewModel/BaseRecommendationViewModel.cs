using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel
{
    public enum RecommendationStatus
    {
        Success,
        Warning,
        Error
    }

    public abstract class BaseRecommendationViewModel : NotifyPropertyChanged
    {
        private readonly Lazy<Geometry> iconProvider;
        private bool isExpanded;
        private string summary;
        private RecommendationStatus status;

        protected BaseRecommendationViewModel()
        {
            this.iconProvider = new Lazy<Geometry>(this.GetIcon);
        }

        public abstract string Title { get; }

        public Geometry Icon
        {
            get => this.iconProvider.Value;
        }

        protected abstract Geometry GetIcon();

        public  string Summary
        {
            get => this.summary;
            protected set => this.SetField(ref this.summary, value);
        }

        public RecommendationStatus Status
        {
            get => status;
            protected set => this.SetField(ref this.status, value);
        }

        public virtual bool IsEnabled => true;

        public abstract Task Refresh(CancellationToken cancellationToken = default);

        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.SetField(ref this.isExpanded, value);
        }
    }
}
