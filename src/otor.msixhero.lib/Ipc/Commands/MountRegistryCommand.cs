using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace otor.msixhero.lib.Ipc.Commands
{
    [Serializable]
    public class MountRegistryCommand : BaseCommand
    {
        [XmlElement]
        public string HiveName { get; set; }

        [XmlElement]
        public string Source { get; set; }
    }
}
