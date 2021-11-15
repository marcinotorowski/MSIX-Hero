using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit.Bulk
{
    [Verb("list", HelpText = "Executes a list of commands, either from a file or from a command prompt.")]
    public class ListEditVerb : BaseEditVerb
    {
        [Value(0, HelpText = "File path to a file containing the instructions to execute. If not provided, the user can prompt the required content from the console.")]
        public string File { get; set; }
    }
}
