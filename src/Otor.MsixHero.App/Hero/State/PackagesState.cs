using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.State
{
    public class PackagesState
    {
        public PackagesState()
        {
            this.AllPackages = new List<InstalledPackage>();
            this.SelectedPackages = new List<InstalledPackage>();
            this.Mode = PackageContext.CurrentUser;
        }

        public List<InstalledPackage> AllPackages { get; }

        public List<InstalledPackage> SelectedPackages { get; }

        public PackageFilter Filter { get; set; }
        
        public string SearchKey { get; set; }

        public PackageContext Mode { get; set; }

        public PackageSort SortMode { get; set; }

        public PackageGroup GroupMode { get; set; }

        public bool SortDescending { get; set; }

        public bool ShowSidebar { get; set; }

        public IReadOnlyList<string> ActivePackageNames { get; set; }

        public AppxPackageArchitecture Platform { get; set; }
    }
}