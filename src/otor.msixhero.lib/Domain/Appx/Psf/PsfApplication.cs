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
        
        [DataMember(Name = "startScript", EmitDefaultValue = false)]
        public PsfScript StartScript { get; set; }
        
        [DataMember(Name = "endScript", EmitDefaultValue = false)]
        public PsfScript EndScript { get; set; }

        /// <summary>
        /// Specifies whether to exit the application if the starting script fails.
        /// </summary>
        [DataMember(Name = "stopOnScriptError")]
        public bool StopOnScriptError { get; set; }

        public override string ToString()
        {
            return $"{this.Id} ({this.Executable} {this.Arguments})";
        }
    }
}