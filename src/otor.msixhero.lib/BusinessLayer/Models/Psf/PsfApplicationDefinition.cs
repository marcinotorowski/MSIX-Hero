using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Models.Psf
{
    [Serializable]
    public class PsfApplicationDefinition
    {
        public string Executable { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public List<PsfFileRedirection> FileRedirections { get; set; }

        public PsfBitness? Tracing { get; set; }

        public List<string> OtherFixups { get; set; }
    }
}
