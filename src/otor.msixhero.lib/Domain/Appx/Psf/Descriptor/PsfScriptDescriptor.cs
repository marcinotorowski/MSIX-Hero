using System;

namespace otor.msixhero.lib.Domain.Appx.Psf.Descriptor
{
    public enum PsfScriptDescriptorTiming
    {
        Start,
        Finish
    }


    [Serializable]
    public class PsfScriptDescriptor
    {
        private readonly PsfScript script;
        private readonly PsfScriptDescriptorTiming timing;

        public PsfScriptDescriptor(PsfScript script, PsfScriptDescriptorTiming timing)
        {
            this.script = script;
            this.timing = timing;
        }

        public string Name => this.script.ScriptPath;

        public string Arguments => this.script.ScriptArguments;

        public PsfScriptDescriptorTiming Timing => this.timing;

        public bool OnlyOnce => this.script.RunOnce;
    }
}