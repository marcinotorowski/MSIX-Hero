using System;
using System.Collections.Generic;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    [Serializable]
    public class PsfApplicationDescriptor
    {
        public string Executable { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public List<PsfFolderRedirectionDescriptor> FileRedirections { get; set; }

        public List<PsfScriptDescriptor> Scripts { get; set; }

        public PsfTracingRedirectionDescriptor Tracing { get; set; }

        public PsfElectronDescriptor Electron { get; set; }

        public List<string> OtherFixups { get; set; }
    }
}
