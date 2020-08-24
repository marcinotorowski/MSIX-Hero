using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class SetPackageFilter : VoidCommand
    {
        public SetPackageFilter()
        {
        }

        public SetPackageFilter(PackageFilter filter, string searchKey, AddonsFilter addons)
        {
            this.Filter = filter;
            this.SearchKey = searchKey;
            this.AddonsFilter = addons;
        }

        public AddonsFilter AddonsFilter { get; set; }

        /// <summary>
        /// The filter, or <c>null</c> if the filter is not to be changed by this action.
        /// </summary>
        public PackageFilter Filter { get; set; }

        /// <summary>
        /// The search key, or <c>null</c> if the search key is not to be changed by this action.
        /// </summary>
        public string SearchKey { get; set; }

        public static SetPackageFilter CreateFrom(string searchKey)
        {
            return new SetPackageFilter(PackageFilter.All, searchKey, AddonsFilter.OnlyMain);
        }

        public static SetPackageFilter CreateFrom(PackageFilter packageFilter, string searchKey = null, AddonsFilter addons = AddonsFilter.OnlyMain)
        {
            return new SetPackageFilter(packageFilter, searchKey, addons);
        }

        public static SetPackageFilter CreateFrom(bool systemApps, bool storeApps, bool developerApps, string searchKey = null, AddonsFilter addons = AddonsFilter.OnlyMain)
        {
            PackageFilter flags = 0;

            if (systemApps)
            {
                flags |= PackageFilter.System;
            }

            if (storeApps)
            {
                flags |= PackageFilter.Store;
            }

            if (developerApps)
            {
                flags |= PackageFilter.Developer;
            }

            return CreateFrom(flags, searchKey, addons);
        }
    }
}
