using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("setRegistryValue", HelpText = "CLI_Verbs_Edit_SetRegistryValue_VerbName", ResourceType = typeof(Localization))]
    public class SetRegistryValueEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('k', "key", HelpText = "CLI_Verbs_Edit_SetRegistryValue_Prop_RegistryKey", Required = true, ResourceType = typeof(Localization))]
        public string RegistryKey { get; set; }
        
        [Option('n', "valueName", HelpText = "CLI_Verbs_Edit_SetRegistryValue_Prop_RegistryValueName", Required = true, ResourceType = typeof(Localization))]
        public string RegistryValueName { get; set; }

        [Option('t', "type", HelpText = "CLI_Verbs_Edit_SetRegistryValue_Prop_ValueType", Required = false, ResourceType = typeof(Localization))]
        public string ValueType { get; set; }

        [Value(0, HelpText = "CLI_Verbs_Edit_SetRegistryValue_Prop_Value", ResourceType = typeof(Localization))]
        public string Value { get; set; }
    }
}