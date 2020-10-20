using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.Model
{
    public class RootDependencyVertex : DependencyVertex
    {
        private readonly AppxPackage packageDependency;

        public RootDependencyVertex(AppxPackage packageDependency)
        {
            this.packageDependency = packageDependency;
        }

        public byte[] Logo => this.packageDependency.Logo;

        public string TileColor => "#486984";

        public string DisplayName => this.packageDependency.DisplayName;

        public string Architecture => this.packageDependency.ProcessorArchitecture.ToString();

        public string Version => this.packageDependency.Version;

        public string PublisherDisplayName => this.packageDependency.PublisherDisplayName;
    }
}