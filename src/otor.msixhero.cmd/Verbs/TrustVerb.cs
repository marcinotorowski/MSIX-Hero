using CommandLine;

namespace otor.msixhero.cmd.Verbs
{
    [Verb("trust", HelpText = "Import a certificate or a certificate from MSIX to Trusted People store")]
    public class TrustVerb
    {
        [Value(1, Required = true)]
        public string File { get; set; }
    }
}