using System;

namespace Otor.MsixHero.Dependencies.Domain
{
    public class PackageDependency : BasePackageDependency, IPackageVersionDependency
    {
        public PackageDependency(string packageName, string publisher, Version minVersion) : base(packageName)
        {
            this.Publisher = publisher;
            this.MinVersion = minVersion;
        }

        public string Publisher { get; }

        public Version MinVersion { get; }
    }
}