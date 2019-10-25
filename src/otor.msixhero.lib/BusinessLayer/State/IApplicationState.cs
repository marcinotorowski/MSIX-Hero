using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IApplicationState
    {
        IPackageListState Packages { get; }    

        IEventAggregator EventAggregator { get; }

        ILocalSettings LocalSettings { get; }

        bool IsElevated { get; }
    }
}