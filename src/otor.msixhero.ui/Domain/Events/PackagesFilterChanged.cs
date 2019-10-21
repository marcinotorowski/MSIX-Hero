using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Domain.State;
using MSI_Hero.Domain.State.Enums;
using Prism.Events;

namespace MSI_Hero.Domain.Events
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
