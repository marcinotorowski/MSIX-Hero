using Prism.Events;

namespace MSI_Hero.Domain.State
{
    public class ApplicationState : IApplicationState
    {
        public ApplicationState(IEventAggregator eventAggregator)
        {
            this.Packages = new PackageListState(this);
            this.EventAggregator = eventAggregator;
            this.LocalSettings = new LocalSettings();
        }

        public IEventAggregator EventAggregator { get; }

        public PackageListState Packages { get; }

        public LocalSettings LocalSettings { get; }

        IPackageListState IApplicationState.Packages => this.Packages;

        ILocalSettings IApplicationState.LocalSettings => this.LocalSettings;
    }
}
