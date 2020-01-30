using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public class PsfTraceFixupConfig : PsfFixupConfig
    {
        [DataMember(Name = "traceMethod")]
        public string TraceMethod { get; set; }
        
        [DataMember(Name = "traceLevels")]
        public PsfTraceFixupLevels TraceLevels { get; set; }
        
        [DataMember(Name = "breakOn")]
        public TraceLevel BreakOn { get; set; }

        public override string ToString()
        {
            return $"Trace method {this.TraceMethod}, break on {this.BreakOn}";
        }
    }
}