using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class SetPackageGrouping : CommandWithOutput<PackageGroup>
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
