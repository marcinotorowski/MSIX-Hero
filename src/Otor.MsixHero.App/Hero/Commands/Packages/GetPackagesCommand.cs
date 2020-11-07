using System.Collections.Generic;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class GetPackagesCommand : UiCommand<IList<InstalledPackage>>
    {
        public PackageFindMode? FindMode { get; }

        public GetPackagesCommand()
        {
        }

        public GetPackagesCommand(PackageFindMode findMode)
        {
            this.FindMode = findMode;
        }
    }
}