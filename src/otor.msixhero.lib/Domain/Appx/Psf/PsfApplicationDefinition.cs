using System;
using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Psf.Descriptor;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [Serializable]
    public class PsfApplicationDefinition
    {
        public string Executable { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public List<PsfFolderRedirection> FileRedirections { get; set; }

        public PsfBitness? Tracing { get; set; }

        public List<string> OtherFixups { get; set; }
    }
}
