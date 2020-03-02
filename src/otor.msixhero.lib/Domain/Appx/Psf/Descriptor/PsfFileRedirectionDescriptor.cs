using System;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    [Serializable]
    public class PsfFileRedirectionDescriptor
    {
        public string RegularExpression { get; set; }

        public string DisplayName { get; set; }
    }
}