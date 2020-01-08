using System;
using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Manager
{
    [Serializable]
    public class RemovePackages : SelfElevatedCommand
    {
        public RemovePackages()
        {
            this.Packages = new List<InstalledPackage>();
        }

        public RemovePackages(PackageContext context, IEnumerable<InstalledPackage> packages) : this()
        {
            this.Context = context;
            this.Packages.AddRange(packages);
        }

        public RemovePackages(IEnumerable<InstalledPackage> packages) : this(PackageContext.CurrentUser, packages)
        {
        }

        public RemovePackages(PackageContext context, params InstalledPackage[] packages) : this(context, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public RemovePackages(params InstalledPackage[] packages) : this(PackageContext.CurrentUser, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public List<InstalledPackage> Packages { get; set; }

        public PackageContext Context { get; set;  }

        public bool RemoveAppData { get; set; }
        
        public override bool RequiresElevation => this.Context == PackageContext.AllUsers;
    }
}
