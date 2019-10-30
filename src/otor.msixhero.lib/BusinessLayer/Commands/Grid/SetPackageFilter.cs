using System;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{
    [Serializable]
    public class SetPackageFilter : BaseCommand
    {
        public SetPackageFilter()
        {
        }

        public SetPackageFilter(PackageFilter filter, string searchKey)
        {
            this.Filter = filter;
            this.SearchKey = searchKey;
        }

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
            return new SetPackageFilter(PackageFilter.All, searchKey);
        }

        public static SetPackageFilter CreateFrom(PackageFilter packageFilter, string searchKey = null)
        {
            return new SetPackageFilter(packageFilter, searchKey);
        }

        public static SetPackageFilter CreateFrom(bool systemApps, bool storeApps, bool developerApps, string searchKey = null)
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

            return CreateFrom(flags, searchKey);
        }
    }
}
