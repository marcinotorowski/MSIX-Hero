using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.Dependencies.Domain
{
    public class ResolvedMainPackageDependency : MainPackageDependency, IResolvedPackageDependency
    {
        public ResolvedMainPackageDependency(AppxPackage package) : base(package.Name)
        {
            this.InstalledVersion = System.Version.Parse(package.Version);
            this.Publisher = package.Publisher;
            this.DisplayName = package.DisplayName;
            this.PublisherDisplayName = package.PublisherDisplayName;
            this.Package = package;
        }

        public string Publisher { get; }

        public string DisplayName { get; }

        public string PublisherDisplayName { get; }

        public Version InstalledVersion { get; }

        public AppxPackage Package { get; }
    }
}