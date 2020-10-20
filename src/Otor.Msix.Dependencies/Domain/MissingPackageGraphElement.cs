namespace Otor.Msix.Dependencies.Domain
{
    public class MissingPackageGraphElement : GraphElement
    {
        public MissingPackageGraphElement(int id, string packageName) : base(id)
        {
            this.PackageName = packageName;
        }

        public string PackageName { get; }

        public override string ToString()
        {
            return this.PackageName;
        }
    }
}