using System;
using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Commands.Manager
{
    [Serializable]
    public class RemovePackages : SelfElevatedCommand
    {
        public RemovePackages()
        {
            this.Packages = new List<Package>();
        }

        public RemovePackages(PackageContext context, IEnumerable<Package> packages) : this()
        {
            this.Context = context;
            this.Packages.AddRange(packages);
        }

        public RemovePackages(IEnumerable<Package> packages) : this(PackageContext.CurrentUser, packages)
        {
        }

        public RemovePackages(PackageContext context, params Package[] packages) : this(context, (IEnumerable<Package>)packages)
        {
        }

        public RemovePackages(params Package[] packages) : this(PackageContext.CurrentUser, (IEnumerable<Package>)packages)
        {
        }

        public List<Package> Packages { get; set; }

        public PackageContext Context { get; set;  }

        public bool RemoveAppData { get; set; }
        
        public override bool RequiresElevation => this.Context == PackageContext.AllUsers;
    }
}
