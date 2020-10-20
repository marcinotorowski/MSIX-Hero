using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.Msix.Dependencies.Domain
{
    public interface IResolvedPackageDependency : IPackageDependency
    {
        public string DisplayName { get; }

        public string PublisherDisplayName { get; }

        public Version InstalledVersion { get; }

        AppxPackage Package { get; }
    }
}