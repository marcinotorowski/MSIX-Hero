using System;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities.Descriptor
{
    public enum TracingType
    {
        Console,
        EventLog,
        Default
    }

    [Serializable]
    public class PsfTracingRedirectionDescriptor
    {
        private readonly PsfTraceFixupConfig traceFixupConfig;

        // For serialization
        public PsfTracingRedirectionDescriptor()
        {
        }

        public PsfTracingRedirectionDescriptor(PsfTraceFixupConfig traceFixupConfig, PsfBitness psfBitness)
        {
            PsfBitness = psfBitness;
            this.traceFixupConfig = traceFixupConfig;

            var methods = traceFixupConfig.TraceMethod?.ToLowerInvariant();
            this.BreakOn = traceFixupConfig.BreakOn;


            if (string.IsNullOrEmpty(methods))
            {
                return;
            }

            switch (methods)
            {
                case "printf":
                    this.TracingType = TracingType.Console;
                    break;
                case "eventlog":
                    this.TracingType = TracingType.EventLog;
                    break;
                default:
                    this.TracingType = TracingType.Default;
                    break;
            }
        }

        [DataMember(Name = "psfBitness")]
        public PsfBitness PsfBitness { get; set; }

        [DataMember(Name = "tracingType")]
        public TracingType TracingType { get; set; }


        [DataMember(Name = "breakOn")]
        public TraceLevel BreakOn { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
