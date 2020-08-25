using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
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