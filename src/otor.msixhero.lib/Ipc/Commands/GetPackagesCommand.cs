using System;
using System.Xml.Serialization;
using otor.msihero.lib;

namespace otor.msixhero.lib.Ipc.Commands
{
    [Serializable]
    public class GetPackagesCommand : BaseCommand
    {
        [XmlElement]
        public PackageFindMode Mode { get; set; }
    }
}
