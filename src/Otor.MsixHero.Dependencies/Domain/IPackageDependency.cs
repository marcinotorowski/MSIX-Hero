namespace Otor.MsixHero.Dependencies.Domain
{
    public interface IPackageDependency : IPackageNameDependency
    {
        public string Publisher { get; }
    }
}