using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public class PsfRedirectedPathEntryConfig : JsonElement
    {
        [DataMember(Name = "isReadOnly")]
        public bool IsReadOnly { get; set; }

        [DataMember(Name = "base")]
        public string Base { get; set; }

        [DataMember(Name = "patterns")]
        public List<string> Patterns { get; set; }

        [DataMember(Name = "isExclusion")]
        public bool IsExclusion { get; set; }

        [DataMember(Name = "redirectTargetBase")]
        public bool RedirectTargetBase { get; set; }

        public override string ToString()
        {
            if (this.IsExclusion)
            {
                return $"Exclude {this.Base}\\{this.Patterns?.Count ?? 0} patterns";
            }
            else
            {
                return $"Include {this.Base}\\{this.Patterns?.Count ?? 0} patterns";
            }
        }
    }
}