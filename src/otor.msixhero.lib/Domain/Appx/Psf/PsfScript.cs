using System.ComponentModel;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    [DataContract]
    public class PsfScript : JsonElement
    {
        /// <summary>
        /// The path to the script including the name and extension.The path starts from the root directory of the application.
        /// </summary>
        [DataMember(Name = "scriptPath")]
        public string ScriptPath { get; set; }

        /// <summary>
        /// Space delimited argument list.The format is the same for a PowerShell script call. This string gets appended to scriptPath to make a valid PowerShell.exe call.
        /// </summary>
        [DataMember(Name = "scriptArguments")]
        public string ScriptArguments { get; set; }

        /// <summary>
        /// Specifies whether the script should run in the same virtual environment that the packaged application runs in.
        /// </summary>
        [DataMember(Name = "runInVirtualEnvironment")]
        [DefaultValue(true)]
        public bool RunInVirtualEnvironment { get; set; }

        /// <summary>
        /// Specifies whether the script should run once per user, per version.
        /// </summary>
        [DataMember(Name = "runOnce")]
        [DefaultValue(true)]
        public bool RunOnce { get; set; }

        /// <summary>
        /// Specifies whether the packaged application should wait for the starting script to finish before starting.
        /// </summary>
        [DataMember(Name = "waitForScriptToFinish")]
        [DefaultValue(true)]
        public bool WaitForScriptToFinish { get; set; }

        /// <summary>
        /// Specifies whether the PowerShell window is shown.
        /// </summary>
        [DataMember(Name = "showWindow")]
        public bool ShowWindow { get; set; }

        /// <summary>
        /// How long the script will be allowed to execute. When the time elapses, the script will be stopped.
        /// </summary>
        [DataMember(Name = "timeout")]
        public uint Timeout { get; set; }
    }
}