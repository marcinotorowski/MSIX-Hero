using System;
using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class RemoveDto : ProxyObject
    {
        public RemoveDto()
        {
            this.Packages = new List<InstalledPackage>();
        }

        public RemoveDto(PackageContext context, IEnumerable<InstalledPackage> packages) : this()
        {
            this.Context = context;
            this.Packages.AddRange(packages);
        }

        public RemoveDto(IEnumerable<InstalledPackage> packages) : this(PackageContext.CurrentUser, packages)
        {
        }

        public RemoveDto(PackageContext context, params InstalledPackage[] packages) : this(context, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public RemoveDto(params InstalledPackage[] packages) : this(PackageContext.CurrentUser, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public List<InstalledPackage> Packages { get; set; }

        public PackageContext Context { get; set;  }

        public bool RemoveAppData { get; set; }
    }
}
