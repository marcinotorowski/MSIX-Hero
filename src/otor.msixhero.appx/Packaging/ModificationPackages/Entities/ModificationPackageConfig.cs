using System.IO;

namespace Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities
{
    public class ModificationPackageConfig
    {
        public string ParentName { get; set; }

        public string ParentPublisher { get; set; }

        public string Name { get; set; }
        
        public string DisplayName { get; set; }

        public string DisplayPublisher { get; set; }

        public string Publisher { get; set; }

        public string Version { get; set; }
        
        public string ParentPackagePath { get; set; }

        public bool IncludeVfsFolders { get; set; }

        public DirectoryInfo IncludeFolder { get; set; }

        public FileInfo IncludeRegistry { get; set; }
    }
}
