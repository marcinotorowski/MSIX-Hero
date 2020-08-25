using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
{
    public class PackageGroupAndSortChangedPayload
    {
        public PackageGroupAndSortChangedPayload(PackageGroup grouping, PackageSort sorting, bool sortDescending)
        {
            this.Grouping = grouping;
            this.Sorting = sorting;
            this.SortingDescending = sortDescending;
        }

        public PackageGroup Grouping { get; private set; }

        public PackageSort Sorting { get; private set; }

        public bool SortingDescending { get; private set; }
    }
}