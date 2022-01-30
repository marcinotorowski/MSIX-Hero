using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Bulk
{
    [Verb("list", HelpText = "CLI_Verbs_Edit_List_VerbName", ResourceType = typeof(Localization))]
    public class ListEditVerb : BaseEditVerb
    {
        [Value(0, HelpText = "CLI_Verbs_Edit_List_Prop_File", ResourceType = typeof(Localization))]
        public string File { get; set; }
    }
}
