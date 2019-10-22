using Prism.Events;

namespace MSI_Hero.Domain.State
{
    public interface IApplicationState
    {
        IPackageListState Packages { get; }    

        IEventAggregator EventAggregator { get; }

        ILocalSettings LocalSettings { get; }
    }
}