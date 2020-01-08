using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.State
{
    public interface IWritablePackageListState
    {
        bool ShowSidebar { get; set; }

        PackageFilter Filter { get; set; }

        PackageGroup Group { get; set; }

        PackageSort Sort { get; set; }

        bool SortDescending { get; set; }

        PackageContext Context { get; set; }

        string SearchKey { get; set; }

        List<InstalledPackage> HiddenItems { get; }

        List<InstalledPackage> VisibleItems { get; }

        List<InstalledPackage> SelectedItems { get; }
    }
}