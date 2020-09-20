using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
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