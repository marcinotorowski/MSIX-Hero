using System;
using System.IO;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging.ManifestCreator
{
    public class AppxManifestCreatorOptions
    {
        public string[] EntryPoints { get; set; }

        public FileInfo RegistryFile { get; set; }
        
        public AppxPackageArchitecture PackageArchitecture { get; set; }

        public string PackageName { get; set; }

        public string PackageDisplayName { get; set; }

        public string PublisherName { get; set; }

        public string PublisherDisplayName { get; set; }

        public Version Version { get; set; }

        public bool CreateLogo { get; set; }

        public static AppxManifestCreatorOptions Default =>
            new AppxManifestCreatorOptions
            {
                PackageArchitecture = AppxPackageArchitecture.Neutral,
                RegistryFile = null,
                Version = null,
                PackageName = "MyPackage",
                PackageDisplayName = "My package",
                PublisherName = "CN=Publisher",
                PublisherDisplayName = "Publisher",
                CreateLogo = true
            };
    }
}