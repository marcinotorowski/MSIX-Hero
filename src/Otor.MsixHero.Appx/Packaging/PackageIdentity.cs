using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging
{
    public class PackageIdentity
    {
        public string AppName { get; set; }

        public Version AppVersion { get; set; }

        public AppxPackageArchitecture Architecture { get; set; }

        public string PublisherHash { get; set; }

        public string ResourceId { get; set; }

        public static PackageIdentity FromFullName(string packageFullName)
        {
            if (packageFullName == null)
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            var split = packageFullName.Split('_');
            if (split.Length < 5)
            {
                throw new ArgumentException("The full name must have 5 units.", nameof(packageFullName));
            }

            var obj = new PackageIdentity
            {
                AppName = split[0],
                AppVersion = Version.Parse(split[1]),
                Architecture = (AppxPackageArchitecture) Enum.Parse(typeof(AppxPackageArchitecture), split[2], true),
                ResourceId = split[3],
                PublisherHash = split[4]
            };

            return obj;
        }

        public override string ToString()
        {
            return $"{this.AppName}_{this.AppVersion}_{this.Architecture:G}_{this.ResourceId}_{this.PublisherHash}";
        }
    }
}
