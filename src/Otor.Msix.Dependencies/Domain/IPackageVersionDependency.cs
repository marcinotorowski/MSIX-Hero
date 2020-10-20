using System;

namespace Otor.Msix.Dependencies.Domain
{
    public interface IPackageVersionDependency : IPackageDependency
    {
        public Version MinVersion { get; }
    }
}