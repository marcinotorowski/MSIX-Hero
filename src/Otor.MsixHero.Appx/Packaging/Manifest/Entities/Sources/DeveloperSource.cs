using System.IO;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources
{
    public class DeveloperSource : AppxSource
    {
        public DeveloperSource(string manifestPath) : base(Path.GetDirectoryName(manifestPath))
        {
            this.ManifestPath = manifestPath;
        }

        public string ManifestPath { get; }
    }
}