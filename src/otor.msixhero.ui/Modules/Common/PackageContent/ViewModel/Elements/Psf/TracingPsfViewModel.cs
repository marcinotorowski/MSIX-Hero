using System;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
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