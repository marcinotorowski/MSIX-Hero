using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using Windows.Data.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public class PsfConfigSerializer
    {
        public PsfConfig Deserialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<PsfConfig>(jsonString);
        }
    }


    [DataContract]
    public class PsfApplication
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "executable")]
        public string Executable { get; set; }

        [DataMember(Name = "workingDirectory")]
        public string WorkingDirectory { get; set; }

        [DataMember(Name = "arguments")]
        public string Arguments { get; set; }
    }

    [DataContract]
    public class PsfProcess
    {
        [DataMember(Name = "executable")]
        public string ExecutableRegexPattern { get; set; }

        [DataMember(Name = "fixups")]
        public List<PsfFixup> Fixups { get; set; }
    }

    [DataContract]
    public class PsfFixup
    {
        [DataMember(Name = "dll")]
        public string Dll { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> RawConfig { get; set; }

        [JsonIgnore]
        public PsfFixupConfig Config { get; set; }
        
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (this.Config == null)
            {
                this.RawConfig = null;
                return;
            }

            this.RawConfig = JObject.FromObject(this.Config);
        }

        [OnDeserialized]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            if (!this.RawConfig.ContainsKey("config"))
            {
                return;
            }

            if (this.Dll?.StartsWith("FileRedirectionFixup", StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Config = JsonConvert.DeserializeObject<PsfRedirectionFixupConfig>(this.RawConfig["config"].ToString(Formatting.None));
            }
            else if (this.Dll?.StartsWith("TraceFixup", StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Config = JsonConvert.DeserializeObject<PsfTraceFixupConfig>(this.RawConfig["config"].ToString(Formatting.None));
            }
        }
    }

    public class PsfRedirectedPathConfig
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageRelative")]
        public List<PsfRedirectedPathRelativeConfig> PackageRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageDriveRelative")]
        public List<PsfRedirectedPathRelativeConfig> PackageDriveRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "knownFolder")]
        public List<PsfRedirectedPathRelativeConfig> KnownFolder { get; set; }
    }

    public abstract class PsfFixupConfig
    {
    }

    [DataContract]
    public class PsfTraceFixupLevels
    {
        [JsonExtensionData]
        public IDictionary<string, JObject> rawValues;

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
    }

    [DataContract]
    public class PsfRedirectionFixupConfig : PsfFixupConfig
    {
        [JsonExtensionData]
        public IDictionary<string, JObject> rawValues;

        [DataMember(Name = "redirectedPaths")]
        public PsfRedirectedPathConfig RedirectedPaths { get; set; }
    }

    [DataContract]
    public class PsfRedirectedPathRelativeConfig
    {
        [JsonExtensionData]
        public IDictionary<string, JObject> rawValues;

        [DataMember(Name = "base")]
        public string Base { get; set; }

        [DataMember(Name = "patterns")]
        public List<string> Patterns { get; set; }
    }


    public class PsfTraceFixupConfig : PsfFixupConfig
    {
        [JsonExtensionData]
        public IDictionary<string, JObject> rawValues;

        [DataMember(Name = "traceMethod")]
        public string TraceMethod { get; set; }
        
        [DataMember(Name = "traceLevels")]
        public TraceLevel TraceLevels { get; set; }
        
        [DataMember(Name = "breakOn")]
        public TraceLevel BreakOn { get; set; }
    }

    public enum TraceLevel
    {
        [EnumMember(Value = "always")]
        Always,

        [EnumMember(Value = "ignoreSuccess")]
        IgnoreSuccess,

        [EnumMember(Value = "allFailures")]
        AllFailures,

        [EnumMember(Value = "unexpectedFailures")]
        UnexpectedFailures,

        [EnumMember(Value = "ignore")]
        Ignore
    }

    [DataContract]
    public class PsfConfig
    {
        [JsonExtensionData]
        public IDictionary<string, JObject> rawValues;

        [DataMember(Name = "applications")]
        public List<PsfApplication> Applications { get; set; }

        [DataMember(Name = "processes")]
        public List<PsfProcess> Processes { get; set; }
    }
}
