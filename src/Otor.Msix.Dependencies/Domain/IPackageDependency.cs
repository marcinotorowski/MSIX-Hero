namespace Otor.Msix.Dependencies.Domain
{
    public interface IPackageDependency : IPackageNameDependency
    {
        public string Publisher { get; }
    }
}