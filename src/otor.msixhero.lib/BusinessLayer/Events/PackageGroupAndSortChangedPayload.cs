using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Events
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