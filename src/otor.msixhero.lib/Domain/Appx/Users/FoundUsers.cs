using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.State;

namespace otor.msixhero.lib.Domain.Appx.Users
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
