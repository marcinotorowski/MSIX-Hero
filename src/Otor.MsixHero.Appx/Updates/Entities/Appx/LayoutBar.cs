using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    [Serializable]
    public class LayoutBar
    {
        [XmlAttribute("position")]
        public long Position { get; set; }

        [XmlAttribute("size")]
        public long Size { get; set; }
        
        [XmlAttribute("type")]
        public ComparisonStatus Status { get; set; }
    }
}