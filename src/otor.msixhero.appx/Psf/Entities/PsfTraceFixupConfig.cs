using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public class PsfTraceFixupConfig : PsfFixupConfig
    {
        [DataMember(Name = "traceMethod")]
        [DefaultValue("default")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string TraceMethod { get; set; }
        
        [DataMember(Name = "traceLevels")]
        public PsfTraceFixupLevels TraceLevels { get; set; }
        
        [DataMember(Name = "breakOn")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(TraceLevel.UnexpectedFailures)]
        public TraceLevel BreakOn { get; set; }

        public override string ToString()
        {
            return $"Trace method {this.TraceMethod}, break on {this.BreakOn}";
        }
    }
}