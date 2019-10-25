using otor.msixhero.lib.BusinessLayer.State;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
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

        public bool IsElevated { get; } = UserHelper.IsAdministrator();
    }
}
