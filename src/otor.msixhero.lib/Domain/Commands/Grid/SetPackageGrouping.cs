using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Grid
{
    [Serializable]
    public class SetPackageGrouping : BaseCommand<PackageGroup>
    {
        public SetPackageGrouping()
        {
        }

        public SetPackageGrouping(PackageGroup groupMode)
        {
            GroupMode = groupMode;
        }

        public PackageGroup GroupMode { get; set; }
    }
}
