using System;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace otor.msixhero.lib.Ipc.Commands
{
    [Serializable]
    public class UnmountRegistryCommand : BaseCommand
    {
        [XmlElement]
        public string HiveName { get; set; }
    }
}