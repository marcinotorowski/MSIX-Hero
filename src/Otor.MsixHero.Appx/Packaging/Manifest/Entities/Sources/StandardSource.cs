using System.IO;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources
{
    public class StandardSource : AppxSource
    {
        public StandardSource(string manifestPath) : base(Path.GetDirectoryName(manifestPath))
        {
            this.ManifestPath = manifestPath;
        }

        public string ManifestPath { get; }
    }
}