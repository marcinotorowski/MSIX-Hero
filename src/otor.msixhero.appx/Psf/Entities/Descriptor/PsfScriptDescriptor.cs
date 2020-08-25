using System;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities.Descriptor
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

        // For serialization
        public PsfScriptDescriptor()
        {
        }

        public PsfScriptDescriptor(PsfScript script, PsfScriptDescriptorTiming timing)
        {
            this.script = script;
            this.Timing = timing;
            this.Name = this.script.ScriptPath;
            this.Arguments = this.script.ScriptArguments;
            this.RunInVirtualEnvironment = this.script.RunInVirtualEnvironment;
            this.RunOnce = this.script.RunOnce;
            this.ShowWindow = this.script.ShowWindow;
            this.WaitForScriptToFinish = this.script.WaitForScriptToFinish;
        }

        [DataMember(Name = "scriptPath")]
        public string Name { get; set; }

        [DataMember(Name = "scriptArguments")]
        public string Arguments { get; set; }

        [DataMember(Name = "timing")]
        public PsfScriptDescriptorTiming Timing { get; set; }

        [DataMember(Name = "runOnce")]
        public bool RunOnce { get; set; }

        [DataMember(Name = "runInVirtualEnvironment")]
        public bool RunInVirtualEnvironment { get; set; }

        [DataMember(Name = "waitForScriptToFinish")]
        public bool WaitForScriptToFinish { get; set; }
        
        [DataMember(Name = "showWindow")]
        public bool ShowWindow { get; set; }
    }
}