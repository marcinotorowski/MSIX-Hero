using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.lib.Domain.State
{
    public class PackageListState : IPackageListState, IWritablePackageListState
    {
        public PackageListState()
        {
            this.Context = UserHelper.IsAdministrator() ? PackageContext.AllUsers : PackageContext.CurrentUser;
            this.Filter = PackageFilter.Developer;
            this.Group = PackageGroup.Publisher;
            this.ShowSidebar = true;

            this.VisibleItems = new List<InstalledPackage>();
            this.HiddenItems = new List<InstalledPackage>();
            this.SelectedItems = new List<InstalledPackage>();
        }

        public PackageFilter Filter { get; set; }

        public PackageContext Context { get; set; }

        public PackageGroup Group { get; set; }

        public PackageSort Sort { get; set; }

        public bool ShowSidebar { get; set; }

        public string SearchKey { get; set; }

        public List<InstalledPackage> VisibleItems { get; }

        public List<InstalledPackage> HiddenItems { get; }

        public List<InstalledPackage> SelectedItems { get; }

        public bool SortDescending { get; set; }
        
        IReadOnlyCollection<InstalledPackage> IPackageListState.VisibleItems => this.VisibleItems;

        IReadOnlyCollection<InstalledPackage> IPackageListState.HiddenItems => this.HiddenItems;

        IReadOnlyCollection<InstalledPackage> IPackageListState.SelectedItems => this.SelectedItems;
    }
}