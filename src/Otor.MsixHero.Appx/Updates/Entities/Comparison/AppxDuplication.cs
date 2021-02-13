using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    [Serializable]
    public class AppxDuplication : BaseDuplicateFiles
    {
        [XmlArray("duplicates")]
        [XmlArrayItem("duplicate")]
        public List<ComparedDuplicate> Duplicates { get; set; }
        
        [XmlAttribute("fileSize")]
        public long FileSize { get; set; }

        [XmlAttribute("fileCount")]
        public long FileCount { get; set; }
    }
}