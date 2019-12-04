using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.State
{
    public interface IPackageListState
    {
        bool ShowSidebar { get; }

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