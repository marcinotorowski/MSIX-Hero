using System;
using System.Collections.Generic;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    [Serializable]
    public class PsfFolderRedirectionDescriptor
    {
        public string Directory { get; set; }

        public List<PsfFileRedirectionDescriptor> Inclusions { get; set; }
        
        public List<PsfFileRedirectionDescriptor> Exclusions { get; set; }
    }
}