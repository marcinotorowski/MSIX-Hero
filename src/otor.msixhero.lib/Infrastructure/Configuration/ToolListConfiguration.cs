using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class ToolListConfiguration : BaseJsonSetting
    {
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [DataMember(Name = "path")]
        [JsonProperty(PropertyName = "path", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ResolvableFolder.ResolvablePath Path { get; set; }

        [DataMember(Name = "icon")]
        [JsonProperty(PropertyName = "icon", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ResolvableFolder.ResolvablePath Icon { get; set; }

        [DataMember(Name = "asAdmin")]
        [JsonProperty(PropertyName = "asAdmin")]
        public bool AsAdmin { get; set; }

        [DataMember(Name = "arguments")]
        [JsonProperty(PropertyName = "arguments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Arguments { get; set; }
    }
}