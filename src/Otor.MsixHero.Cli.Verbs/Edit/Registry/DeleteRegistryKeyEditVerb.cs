using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("deleteRegistryKey", HelpText = "CLI_Verbs_Edit_DeleteRegistryKey_VerbName", ResourceType = typeof(Localization))]
    public class DeleteRegistryKeyEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "CLI_Verbs_Edit_DeleteRegistryKey_Prop_RegistryKey", Required = true, ResourceType = typeof(Localization))]
        public string RegistryKey { get; set; }
    }
}