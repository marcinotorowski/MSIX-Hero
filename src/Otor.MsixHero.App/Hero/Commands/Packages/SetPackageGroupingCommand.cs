using System;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.Packages
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
