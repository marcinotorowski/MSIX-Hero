using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msihero.lib;

namespace otor.msixhero.lib.Ipc.Commands
{
    [Serializable]
    [XmlInclude(typeof(GetPackagesCommand))]
    [XmlInclude(typeof(MountRegistryCommand))]
    [XmlInclude(typeof(UnmountRegistryCommand))]
    public abstract class BaseCommand
    {
        [XmlElement]
        public List<Package> Result { get; set; }
    }
}
