using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    public class CheckForUpdatesCommand : UiCommand<AppInstallerUpdateAvailabilityResult>
    {
        public CheckForUpdatesCommand()
        {
        }

        public CheckForUpdatesCommand(string packageFullName)
        {
            this.PackageFullName = packageFullName;
        }

        public string PackageFullName { get; }
    }
}