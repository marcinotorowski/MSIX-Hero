using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("deleteRegistryKey", HelpText = "Deletes a specific registry key.")]
    public class DeleteRegistryKeyEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "A registry path (for example HKLM\\Software\\abc) of a registry key to be deleted.", Required = true)]
        public string RegistryKey { get; set; }
    }
}