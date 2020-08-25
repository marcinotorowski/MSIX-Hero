using System;

namespace Otor.MsixHero.Appx.Psf.Entities.Descriptor
{
    [Serializable]
    public class PsfFileRedirectionDescriptor
    {
        public string RegularExpression { get; set; }

        public string DisplayName { get; set; }
    }
}