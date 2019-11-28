using System;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [Serializable]
    public class PsfFileRedirection
    {
        public string Directory { get; set; }

        public string RegularExpression { get; set; }

        public bool IsExclusion { get; set; }
    }
}