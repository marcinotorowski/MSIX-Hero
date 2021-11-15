using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("deleteRegistryValue", HelpText = "Deletes a specific registry value.")]
    public class DeleteRegistryValueEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "A registry path (for example HKLM\\Software\\abc) of a registry key to be deleted.", Required = true)]
        public string RegistryKey { get; set; }
        
        [Option('n', "valueName", HelpText = "A value name to be deleted.", Required = true)]
        public string RegistryValueName { get; set; }
    }
}