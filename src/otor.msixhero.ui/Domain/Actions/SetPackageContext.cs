using MSI_Hero.Domain.State.Enums;

namespace MSI_Hero.Domain.Actions
{

    public class SetPackageContext : IAction
    {
        public SetPackageContext(PackageContext context)
        {
            this.Context = context;
        }

        public PackageContext Context { get; set; }

        public bool ForceReload { get; set; }
    }
}
