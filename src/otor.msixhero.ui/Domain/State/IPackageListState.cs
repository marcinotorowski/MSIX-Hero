using System.Collections.Generic;
using MSI_Hero.Domain.State.Enums;
using otor.msihero.lib;

namespace MSI_Hero.Domain.State
{
    public interface IPackageListState
    {
        PackageFilter Filter { get; }

        PackageContext Context { get; }

        string SearchKey { get; }

        IReadOnlyCollection<Package> HiddenItems { get; }

        IReadOnlyCollection<Package> VisibleItems { get; }

        IReadOnlyCollection<Package> SelectedItems { get; }
    }
}