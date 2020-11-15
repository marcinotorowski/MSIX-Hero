using Otor.MsixHero.Dependencies.Domain;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Model
{
    public class PackageDependencyVertex : DependencyVertex
    {
        private readonly InstalledPackageGraphElement packageGraphDependency;

        public PackageDependencyVertex(InstalledPackageGraphElement packageGraphDependency)
        {
            this.packageGraphDependency = packageGraphDependency;
        }

        public string Logo => this.packageGraphDependency.Package.Image;

        public string TileColor => this.packageGraphDependency.Package.TileColor;

        public string DisplayName => this.packageGraphDependency.Package.DisplayName;

        public string Architecture => this.packageGraphDependency.Package.Architecture;

        public string Version => this.packageGraphDependency.Package.Version.ToString();

        public string PublisherDisplayName => this.packageGraphDependency.Package.DisplayPublisherName;
    }
}