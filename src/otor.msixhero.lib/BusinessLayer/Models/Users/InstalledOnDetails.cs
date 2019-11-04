using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Models.Users
{
    [Serializable]
    public class InstalledOnDetails
    {
        [XmlElement]
        public ElevationStatus Status { get; set; }

        [XmlElement]
        public List<User> Users { get; set; }
    }
}