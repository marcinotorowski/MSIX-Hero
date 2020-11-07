using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class StopPackageCommand : UiCommand
    {
        public StopPackageCommand(InstalledPackage installedPackage)
        {
            this.Package = installedPackage;
        }

        public InstalledPackage Package { get; }
    }
}