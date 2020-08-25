using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events.Volumes
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
