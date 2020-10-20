using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.Msix.Dependencies.Domain
{
    public class SystemDependency : Dependency
    {
        public SystemDependency(AppxTargetOperatingSystem operatingSystem)
        {
            this.OperatingSystem = operatingSystem;
        }

        public AppxTargetOperatingSystem OperatingSystem { get; }
    }
}