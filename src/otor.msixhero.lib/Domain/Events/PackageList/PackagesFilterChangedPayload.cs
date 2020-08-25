using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
{
    public class PackagesFilterChangedPayload
    {
        public PackagesFilterChangedPayload(
            PackageFilter newFilter,
            PackageFilter oldFilter,
            AddonsFilter newAddonsFilter,
            AddonsFilter oldAddonsFilter,
            string newSearchKey, 
            string oldSearchKey)
        {
            this.NewFilter = newFilter;
            this.OldFilter = oldFilter;
            this.NewSearchKey = newSearchKey;
            this.OldSearchKey = oldSearchKey;
            this.NewAddonsFilter = newAddonsFilter;
            this.OldAddonsFilter = oldAddonsFilter;
        }

        public PackageFilter NewFilter { get; private set; }

        public PackageFilter OldFilter { get; private set; }

        public AddonsFilter NewAddonsFilter { get; private set; }

        public AddonsFilter OldAddonsFilter { get; private set; }

        public string NewSearchKey { get; private set; }

        public string OldSearchKey { get; private set; }
    }
}