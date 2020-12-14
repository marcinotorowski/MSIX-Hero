using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("extract-cert", HelpText = "Extracts the certificate file (.CER) from an already signed MSIX package.")]
    public class ExtractCertVerb : BaseVerb
    {
        [Value(1, Required = true)]
        public string File { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "Full file path under which the extracted .CER file will be saved.")]
        public string Output { get; set; }
    }
}