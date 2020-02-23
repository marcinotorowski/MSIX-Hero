using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.Volumes
{
    public enum CollectionChangeType
    {
        Simple,
        Reset
    }


    public class VolumesCollectionChanged : PubSubEvent<VolumesCollectionChangedPayLoad>
    {
    }
}
