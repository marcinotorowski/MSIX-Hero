using CommandLine;

namespace otor.msixhero.cmd.Verbs
{
    [Verb("unpack", HelpText = "Unpack a folder")]
    public class UnpackVerb
    {
        [Option('p', "package", Required = true)]
        public string Package { get; set; }

        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }
    }
}