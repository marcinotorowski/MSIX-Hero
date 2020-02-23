using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Manager
{
    [Serializable]
    public class RunPackage : BaseCommand
    {
        public RunPackage()
        {
        }

        public RunPackage(string packageFamilyName, string manifestPath, string applicationId = null)
        {
            this.PackageFamilyName = packageFamilyName;
            this.ManifestPath = manifestPath;
            this.ApplicationId = applicationId;
        }

        public RunPackage(InstalledPackage package, string applicationId = null) : this(package.PackageFamilyName, package.ManifestLocation, applicationId)
        {
        }
        
        [XmlElement]
        public string PackageFamilyName { get; set; }

        [XmlElement]
        public string ManifestPath { get; set; }

        [XmlElement]
        public string ApplicationId { get; set; }
    }
}
