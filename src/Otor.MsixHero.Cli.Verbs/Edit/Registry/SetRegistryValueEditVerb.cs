using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("setRegistryValue", HelpText = "Set a specific registry value.")]
    public class SetRegistryValueEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "A registry path (for example HKLM\\Software\\abc) of a registry key to be deleted.", Required = true)]
        public string RegistryKey { get; set; }
        
        [Option('n', "valueName", HelpText = "A value name to be deleted.", Required = true)]
        public string RegistryValueName { get; set; }

        [Option('t', "type", HelpText = "Value type.", Required = false)]
        public string ValueType { get; set; }

        [Value(0, HelpText = "DWORD value to be set.")]
        public string Value { get; set; }
    }
}