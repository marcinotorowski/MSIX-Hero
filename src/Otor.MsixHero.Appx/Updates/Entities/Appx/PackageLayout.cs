using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    [Serializable]
    public class PackageLayout
    {
        [XmlArray("files")]
        [XmlArrayItem("file")]
        public List<LayoutBar> Files { get; set; }
        
        [XmlArray("blocks")]
        [XmlArrayItem("block")]
        public List<LayoutBar> Blocks { get; set; }
    }
}