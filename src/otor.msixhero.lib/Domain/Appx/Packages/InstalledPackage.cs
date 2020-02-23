using System;
using System.IO;

namespace otor.msixhero.lib.Domain.Appx.Packages
{
    [Serializable]
    public class InstalledPackage
    {
        public PackageContext Context { get; set; }

        public string PackageId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string DisplayPublisherName { get; set; }

        public string Name { get; set; }

        public string Publisher { get; set; }

        public DateTime InstallDate { get; set; }

        public Version Version { get; set; }

        public MsixPackageType PackageType { get; set; }

        public string InstallLocation { get; set; }

        public string ManifestLocation
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(this.InstallLocation, "AppxManifest.xml");
            }
        }

        public string PsfConfig
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(this.InstallLocation, "config.json");
            }
        }

        public string PackageFamilyName { get; set; }

        public SignatureKind SignatureKind { get; set; }

        public string Image { get; set; }

        public string TileColor { get; set; }

        public RegistryMountState HasRegistry { get; set; }

        public string UserDataPath
        {
            get
            {
                if (this.InstallLocation == null)
                {
                    return null;
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.PackageFamilyName);
            }
        }

        public bool IsProvisioned { get; set; }
        public bool IsFramework { get; set; }
    }
}
