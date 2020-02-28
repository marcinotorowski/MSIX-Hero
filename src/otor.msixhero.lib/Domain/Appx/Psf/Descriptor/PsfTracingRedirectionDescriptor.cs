using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    public enum TracingType
    {
        Console,
        EventLog,
        Default
    }

    public class PsfTracingRedirectionDescriptor
    {
        private readonly PsfTraceFixupConfig traceFixupConfig;

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

        public PsfBitness PsfBitness { get; }

        public TracingType TracingType { get; }

        public TraceLevel BreakOn { get; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
