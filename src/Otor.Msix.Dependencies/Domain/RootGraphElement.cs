using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.Msix.Dependencies.Domain
{
    public class RootGraphElement : GraphElement
    {
        public RootGraphElement(AppxPackage package) : base(0)
        {
            this.Package = package;
        }

        public AppxPackage Package { get; }
    }
}
