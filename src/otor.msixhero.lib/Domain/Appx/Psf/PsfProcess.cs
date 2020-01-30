using System.Collections.Generic;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [DataContract]
    public class PsfProcess : JsonElement
    {
        [DataMember(Name = "executable")]
        public string Executable { get; set; }

        [DataMember(Name = "fixups")]
        public List<PsfFixup> Fixups { get; set; }

        public override string ToString()
        {
            return $"{this.Executable} - {this.Fixups?.Count ?? 0} fix-ups";
        }
    }
}