using System;

namespace otor.msixhero.lib.BusinessLayer.Models.Psf
{
    [Serializable]
    public class PsfFileRedirection
    {
        public string Directory { get; set; }

        public string RegularExpression { get; set; }

        public bool IsExclusion { get; set; }
    }
}