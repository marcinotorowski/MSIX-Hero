using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public class PsfRedirectedPathConfig : JsonElement
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageRelative")]
        public List<PsfRedirectedPathEntryConfig> PackageRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageDriveRelative")]
        public List<PsfRedirectedPathEntryConfig> PackageDriveRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "knownFolders")]
        public List<PsfRedirectedPathKnownFolderEntryConfig> KnownFolders { get; set; }

        public override string ToString()
        {
            return $"{this.PackageRelative?.Count ?? 0} package relative, {this.PackageDriveRelative?.Count ?? 0} package drive relative, {this.KnownFolders?.Count ?? 0} known folder";
        }
    }
}