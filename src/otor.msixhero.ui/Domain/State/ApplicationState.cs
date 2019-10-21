using Prism.Events;

namespace MSI_Hero.Domain.State
{
    public class ApplicationState : IApplicationState
    {
        public ApplicationState(IEventAggregator eventAggregator)
        {
            this.Packages = new PackageListState(this);
            this.EventAggregator = eventAggregator;
        }

        public IEventAggregator EventAggregator { get; }

        public PackageListState Packages { get; }

        IPackageListState IApplicationState.Packages => this.Packages;
    }
}
