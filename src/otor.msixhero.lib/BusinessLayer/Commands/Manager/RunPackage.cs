using System;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace otor.msixhero.lib.BusinessLayer.Commands.Manager
{
    [Serializable]
    public class RunPackage : BaseCommand
    {
        public RunPackage()
        {
        }

        public RunPackage(string packageFamilyName, string manifestPath)
        {
            this.PackageFamilyName = packageFamilyName;
            this.ManifestPath = manifestPath;
        }

        [XmlElement]
        public string PackageFamilyName { get; set; }

        [XmlElement]
        public string ManifestPath { get; set; }
    }
}
