using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    [Serializable]
    public class PsfFolderRedirection
    {
        public string Directory { get; set; }

        public List<PsfFileRedirection> Inclusions { get; set; }
        
        public List<PsfFileRedirection> Exclusions { get; set; }
    }

    [Serializable]
    public class PsfFileRedirection
    {
        public string RegularExpression { get; set; }
        public string DisplayName { get; set; }
    }
}