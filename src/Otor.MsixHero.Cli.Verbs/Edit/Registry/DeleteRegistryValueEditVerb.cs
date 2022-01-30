using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("deleteRegistryValue", HelpText = "CLI_Verbs_Edit_DeleteRegistryValue_VerbName", ResourceType = typeof(Localization))]
    public class DeleteRegistryValueEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "CLI_Verbs_Edit_DeleteRegistryValue_Prop_RegistryKey", Required = true, ResourceType = typeof(Localization))]
        public string RegistryKey { get; set; }
        
        [Option('n', "valueName", HelpText = "CLI_Verbs_Edit_DeleteRegistryValue_Prop_RegistryValueName", Required = true, ResourceType = typeof(Localization))]
        public string RegistryValueName { get; set; }
    }
}