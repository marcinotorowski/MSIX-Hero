using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [DataContract]
    public class PsfTraceFixupLevels : JsonElement
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "default")]
        public TraceLevel Default { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "filesystem")]
        public TraceLevel Filesystem { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "registry")]
        public TraceLevel Registry { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "processAndThread")]
        public TraceLevel ProcessAndThread { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "dynamicLinkLibrary")]
        public TraceLevel DynamicLinkLibrary { get; set; }

        public override string ToString()
        {
            return
                $"Default: {this.Default}, filesystem: {this.Filesystem}, registry: {this.Registry}, processAndThread: {this.ProcessAndThread}, dynamicLinkLibrary: {this.DynamicLinkLibrary}";
        }
    }
}