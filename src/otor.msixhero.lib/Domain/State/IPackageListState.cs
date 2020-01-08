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

        IReadOnlyCollection<InstalledPackage> HiddenItems { get; }

        IReadOnlyCollection<InstalledPackage> VisibleItems { get; }

        IReadOnlyCollection<InstalledPackage> SelectedItems { get; }
    }
}