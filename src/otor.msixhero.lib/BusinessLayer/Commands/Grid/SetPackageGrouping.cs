using System;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
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
