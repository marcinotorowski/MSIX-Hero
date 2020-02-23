using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.PackageList
{
    public enum CollectionChangeType
    {
        Simple,
        Reset
    }


    public class PackagesCollectionChanged : PubSubEvent<PackagesCollectionChangedPayLoad>
    {
    }
}
