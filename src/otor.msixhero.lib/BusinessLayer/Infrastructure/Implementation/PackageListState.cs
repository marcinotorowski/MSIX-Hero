using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class PackageListState : IPackageListState
    {
        public PackageListState(ApplicationState parentState)
        {
            this.Context = UserHelper.IsAdministrator() ? PackageContext.AllUsers : PackageContext.CurrentUser;
            this.Filter = PackageFilter.Developer;

            this.VisibleItems = new List<Package>();
            this.HiddenItems = new List<Package>();
            this.SelectedItems = new List<Package>();
        }

        public PackageFilter Filter { get; set; }

        public PackageContext Context { get; set; }

        public string SearchKey { get; set; }

        public List<Package> VisibleItems { get; }

        public List<Package> HiddenItems { get; }

        public List<Package> SelectedItems { get; }

        IReadOnlyCollection<Package> IPackageListState.VisibleItems => this.VisibleItems;

        IReadOnlyCollection<Package> IPackageListState.HiddenItems => this.HiddenItems;

        IReadOnlyCollection<Package> IPackageListState.SelectedItems => this.SelectedItems;
    }
}