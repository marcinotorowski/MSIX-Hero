using System;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Commands
{
    [Serializable]
    public class RemovePackage : BaseSelfElevatedBaseCommand
    {
        public RemovePackage()
        {
        }

        public RemovePackage(Package package, PackageContext context)
        {
            this.Context = context;
            this.Package = package;
        }

        public Package Package { get; set; }

        public PackageContext Context { get; set;  }

        public bool RemoveAppData { get; set; }
        
        public override bool RequiresElevation => this.Context == PackageContext.AllUsers;
    }
}
