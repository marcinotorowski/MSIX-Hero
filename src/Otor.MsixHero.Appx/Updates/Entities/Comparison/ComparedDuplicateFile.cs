using System;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    [Serializable]
    public class ComparedDuplicateFile : BaseDuplicateFiles
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}