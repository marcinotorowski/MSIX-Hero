using otor.msixhero.lib.Domain.Appx.Packages;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.PackageList
{
    public class PackagesFilterChangedPayload
    {
        public PackagesFilterChangedPayload(PackageFilter newFilter, PackageFilter oldFilter, string newSearchKey, string oldSearchKey)
        {
            NewFilter = newFilter;
            OldFilter = oldFilter;
            NewSearchKey = newSearchKey;
            OldSearchKey = oldSearchKey;
        }

        public PackageFilter NewFilter { get; private set; }

        public PackageFilter OldFilter { get; private set; }

        public string NewSearchKey { get; private set; }

        public string OldSearchKey { get; private set; }
    }

    public class PackagesFilterChanged : PubSubEvent<PackagesFilterChangedPayload>
    {
    }
}
