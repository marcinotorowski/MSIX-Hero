using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("setRegistryKey", HelpText = "Sets registry key.")]
    public class SetRegistryKeyEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "A registry path (for example HKLM\\Software\\abc) of a registry key to be set.", Required = true)]
        public string RegistryKey { get; set; }
    }
}