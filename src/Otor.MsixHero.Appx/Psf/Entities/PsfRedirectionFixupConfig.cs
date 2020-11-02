using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfRedirectionFixupConfig : PsfFixupConfig
    {
        [DataMember(Name = "redirectedPaths")]
        public PsfRedirectedPathConfig RedirectedPaths { get; set; }

        public override string ToString()
        {
            return "Redirected paths";
        }
    }
}