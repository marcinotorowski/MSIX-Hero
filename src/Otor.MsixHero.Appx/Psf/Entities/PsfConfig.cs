using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfConfig : JsonElement
    {
        [DataMember(Name = "applications")]
        public List<PsfApplication> Applications { get; set; }

        [DataMember(Name = "processes")]
        public List<PsfProcess> Processes { get; set; }

        public override string ToString()
        {
            return $"{this.Applications?.Count ?? 0} applications, {this.Processes?.Count ?? 0} processes";
        }
    }
}
