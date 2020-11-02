using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Psf.Entities.Descriptor
{
    [Serializable]
    public class PsfFolderRedirectionDescriptor
    {
        public string Directory { get; set; }

        public List<PsfFileRedirectionDescriptor> Inclusions { get; set; }
        
        public List<PsfFileRedirectionDescriptor> Exclusions { get; set; }
    }
}