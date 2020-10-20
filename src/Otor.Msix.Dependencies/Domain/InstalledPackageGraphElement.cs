using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.Msix.Dependencies.Domain
{
    public class InstalledPackageGraphElement : GraphElement
    {
        public InstalledPackageGraphElement(int id, InstalledPackage package) : base(id)
        {
            this.Package = package;
            this.PackageName = package.Name;
        }

        public InstalledPackage Package { get; }

        public string PackageName { get; }

        public override string ToString()
        {
            return this.PackageName;
        }
    }
}