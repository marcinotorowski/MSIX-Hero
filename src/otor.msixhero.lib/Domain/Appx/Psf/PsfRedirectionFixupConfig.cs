using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace otor.msixhero.lib.Domain.Appx.Psf
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