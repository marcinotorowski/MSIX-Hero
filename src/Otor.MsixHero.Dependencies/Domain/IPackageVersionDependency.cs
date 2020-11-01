using System;

namespace Otor.MsixHero.Dependencies.Domain
{
    public interface IPackageVersionDependency : IPackageDependency
    {
        public Version MinVersion { get; }
    }
}