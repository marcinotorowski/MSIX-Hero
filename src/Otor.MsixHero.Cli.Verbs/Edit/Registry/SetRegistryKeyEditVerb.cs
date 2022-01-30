using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("setRegistryKey", HelpText = "CLI_Verbs_Edit_SetRegistryKey_VerbName", ResourceType = typeof(Localization))]
    public class SetRegistryKeyEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "CLI_Verbs_Edit_SetRegistryKey_Prop_RegistryKey", Required = true, ResourceType = typeof(Localization))]
        public string RegistryKey { get; set; }
    }
}