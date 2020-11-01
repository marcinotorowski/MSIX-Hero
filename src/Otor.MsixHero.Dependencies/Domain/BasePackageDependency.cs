namespace Otor.MsixHero.Dependencies.Domain
{
    public abstract class BasePackageDependency : Dependency, IPackageNameDependency
    {
        protected BasePackageDependency(string packageName)
        {
            PackageName = packageName;
        }

        public string PackageName { get; }
    }
}