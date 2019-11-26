using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Commands.Developer
{
    [Serializable]
    public class GetRegistryMountState : BaseCommand<RegistryMountState>
    {
        public GetRegistryMountState()
        {
        }

        public GetRegistryMountState(string installLocation, string packageName)
        {
            this.InstallLocation = installLocation;
            this.PackageName = packageName;
        }

        [XmlElement]
        public string InstallLocation { get; set; }

        [XmlElement]
        public string PackageName { get; set; }
    }
}
