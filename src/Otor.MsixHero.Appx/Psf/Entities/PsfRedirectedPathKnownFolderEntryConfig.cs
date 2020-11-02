using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfRedirectedPathKnownFolderEntryConfig : JsonElement
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "relativePaths")]
        public List<PsfRedirectedPathEntryConfig> RelativePaths { get; set; }

        public override string ToString()
        {
            return $"{this.Id}, {this.RelativePaths?.Count ?? 0} relative paths";
        }
    }
}