using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    [Serializable]
    public class ComparedDuplicate : BaseDuplicateFiles
    {
        [XmlElement("file")]
        public List<ComparedDuplicateFile> Files { get; set; }
    }
}