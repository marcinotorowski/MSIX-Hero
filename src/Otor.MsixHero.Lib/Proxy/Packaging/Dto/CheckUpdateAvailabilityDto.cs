using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class CheckUpdateAvailabilityDto :  ProxyObject<AppInstallerUpdateAvailabilityResult>
    {
        public CheckUpdateAvailabilityDto(string packageFullName)
        {
            this.PackageFullName = packageFullName;
        }

        public CheckUpdateAvailabilityDto()
        {
        }

        public string PackageFullName { get; set; }
    }
}
