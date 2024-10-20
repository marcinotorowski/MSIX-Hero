using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Infrastructure.Configuration;

[DataContract]
public class PackageDirectoryConfiguration : BaseJsonSetting
{
    [DataMember(Name = "path")]
    [JsonProperty(PropertyName = "path", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public ResolvableFolder.ResolvablePath Path { get; set; }

    [DataMember(Name = "isRecurse")]
    [JsonProperty(PropertyName = "isRecurse", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRecurse { get; set; }

    [DataMember(Name = "displayName")]
    [JsonProperty(PropertyName = "displayName", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }
}