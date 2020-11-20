using System;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Volumes.Entities
{
    [Serializable]
    public class AppxVolume
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string PackageStorePath { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        [XmlAttribute]
        public long Capacity { get; set; }

        [XmlAttribute]
        public string DiskLabel { get; set; }

        [XmlAttribute]
        public long AvailableFreeSpace { get; set; }

        public long OccupiedSpace => this.Capacity - this.AvailableFreeSpace;
        public int Percent => this.AvailableFreeSpace == 0 ? 0 : (int)(100 * (float)(Capacity - AvailableFreeSpace) / AvailableFreeSpace);

        [XmlAttribute]
        public bool IsDriveReady { get; set; }

        [XmlAttribute]
        public bool IsOffline { get; set; }

        [XmlAttribute]
        public bool IsSystem { get; set; }
    }
}
