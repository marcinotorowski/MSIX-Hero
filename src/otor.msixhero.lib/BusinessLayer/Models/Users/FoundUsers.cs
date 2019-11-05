using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Models.Users;

namespace otor.msixhero.lib.BusinessLayer.Models.Packages
{
    [Serializable]
    public class FoundUsers
    {
        [XmlElement]
        public ElevationStatus Status { get; set; }

        [XmlElement]
        public List<User> Users { get; set; }
    }
}
