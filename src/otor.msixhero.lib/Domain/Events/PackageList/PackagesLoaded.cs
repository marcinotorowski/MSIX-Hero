using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
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
