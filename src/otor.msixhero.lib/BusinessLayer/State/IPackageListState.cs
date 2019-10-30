using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IPackageListState
    {
        PackageFilter Filter { get; }

        PackageGroup Group { get; }

        PackageSort Sort { get; }

        bool SortDescending { get; }

        PackageContext Context { get; }

        string SearchKey { get; }

        IReadOnlyCollection<Package> HiddenItems { get; }

        IReadOnlyCollection<Package> VisibleItems { get; }

        IReadOnlyCollection<Package> SelectedItems { get; }
    }
}