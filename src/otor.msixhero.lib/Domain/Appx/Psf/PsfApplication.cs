using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [DataContract]
    public class PsfApplication : JsonElement
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "executable")]
        public string Executable { get; set; }

        [DataMember(Name = "workingDirectory")]
        public string WorkingDirectory { get; set; }

        [DataMember(Name = "arguments")]
        public string Arguments { get; set; }

        public override string ToString()
        {
            return $"{this.Id} ({this.Executable} {this.Arguments})";
        }
    }
}