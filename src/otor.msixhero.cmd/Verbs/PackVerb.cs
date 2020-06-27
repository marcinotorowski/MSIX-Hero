using CommandLine;

namespace otor.msixhero.cmd.Verbs
{
    [Verb("pack", HelpText = "Pack a folder")]
    public class PackVerb
    {
        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }

        [Option('p', "package", Required = true)]
        public string Package { get; set; }

        [Option("nc", Default = false, HelpText = "Prevents MSIX Hero from compressing files in the package. By default, files in the package are compressed based on detected file type.")]
        public bool NoCompression { get; set; }

        [Option("nv", Default = false, HelpText = "Skip semantic validation. If you don't specify this option, MSIX Hero performs a full validation of the package.")]
        public bool NoValidation { get; set; }
    }
}
