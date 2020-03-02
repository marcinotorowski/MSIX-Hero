using System;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.lib.Domain.Appx.Psf.Descriptor;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
{
    public class TracingPsfViewModel
    {
        private readonly PsfTracingRedirectionDescriptor definition;

        public TracingPsfViewModel(PsfTracingRedirectionDescriptor definition)
        {
            this.definition = definition;
        }

        public string BreaksOn
        {
            get
            {
                switch (this.definition.BreakOn)
                {
                    case TraceLevel.UnexpectedFailures:
                        return "Unexpected failures";
                    case TraceLevel.Always:
                        return "Always";
                    case TraceLevel.IgnoreSuccess:
                        return "IgnoreSuccess";
                    case TraceLevel.AllFailures:
                        return "All failures";
                    case TraceLevel.Ignore:
                        return "Ignore";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string OutputTo
        {
            get
            {
                switch (this.definition.TracingType)
                {
                    case TracingType.Console:
                        return "Write to console";
                    case TracingType.EventLog:
                        return "Write in Event Log";
                    default:
                        return "Default output";
                }
            }
        }
    }
}