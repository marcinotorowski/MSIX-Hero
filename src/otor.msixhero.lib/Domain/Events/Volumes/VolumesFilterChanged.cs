using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.Volumes
{
    public class VolumesFilterChangedPayload
    {
        public VolumesFilterChangedPayload(string newSearchKey, string oldSearchKey)
        {
            NewSearchKey = newSearchKey;
            OldSearchKey = oldSearchKey;
        }

        public string NewSearchKey { get; private set; }

        public string OldSearchKey { get; private set; }
    }

    public class VolumesFilterChanged : PubSubEvent<VolumesFilterChangedPayload>
    {
    }
}
