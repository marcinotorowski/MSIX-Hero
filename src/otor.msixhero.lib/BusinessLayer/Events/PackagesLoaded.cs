using System.Collections;
using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Events
{
    public enum CollectionChangeType
    {
        Simple,
        Reset
    }

    public class PackagesCollectionChangedPayLoad
    {
        public PackagesCollectionChangedPayLoad(PackageContext packageContext, CollectionChangeType type)
        {
            this.PackageContext = packageContext;
            this.Type = type;

            this.NewPackages = new List<Package>();
            this.OldPackages = new List<Package>();
        }

        public PackageContext PackageContext { get; private set; }

        public CollectionChangeType Type { get; private set; }

        public IList<Package> NewPackages { get; private set; }

        public IList<Package> OldPackages { get; private set; }
    }


    public class PackagesCollectionChanged : PubSubEvent<PackagesCollectionChangedPayLoad>
    {
    }
}
