using System.Collections.Generic;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
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