using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
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