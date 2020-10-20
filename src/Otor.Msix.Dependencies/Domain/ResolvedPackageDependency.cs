using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.Msix.Dependencies.Domain
{
    public class ResolvedPackageDependency : PackageDependency, IResolvedPackageDependency
    {
        public ResolvedPackageDependency(AppxPackage package, Version minVersion) : base(package.Name, package.Publisher, minVersion)
        {
            this.InstalledVersion = System.Version.Parse(package.Version);
            this.DisplayName = package.DisplayName;
            this.PublisherDisplayName = package.PublisherDisplayName;
            this.Package = package;
        }

        public AppxPackage Package { get; }

        public string DisplayName { get; }

        public string PublisherDisplayName { get; }

        public Version InstalledVersion { get; }
    }
}