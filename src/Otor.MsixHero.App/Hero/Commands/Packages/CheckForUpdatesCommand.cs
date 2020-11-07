using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Packages
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