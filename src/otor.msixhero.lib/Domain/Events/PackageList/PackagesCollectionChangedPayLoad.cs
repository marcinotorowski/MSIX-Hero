using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Events.PackageList
{
    public class PackagesCollectionChangedPayLoad
    {
        public PackagesCollectionChangedPayLoad(PackageContext packageContext, CollectionChangeType type)
        {
            this.PackageContext = packageContext;
            this.Type = type;

            this.NewPackages = new List<InstalledPackage>();
            this.OldPackages = new List<InstalledPackage>();
        }

        public PackageContext PackageContext { get; private set; }

        public CollectionChangeType Type { get; private set; }

        public IList<InstalledPackage> NewPackages { get; private set; }

        public IList<InstalledPackage> OldPackages { get; private set; }
    }
}