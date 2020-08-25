using System;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    [Serializable]
    public class SetPackageGroupingCommand : UiCommand<PackageGroup>
    {
        public SetPackageGroupingCommand()
        {
        }

        public SetPackageGroupingCommand(PackageGroup groupMode)
        {
            GroupMode = groupMode;
        }

        public PackageGroup GroupMode { get; set; }
    }
}
