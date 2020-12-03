using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("trust", HelpText = "Import a certificate or a certificate from MSIX to Trusted People store")]
    public class TrustVerb : BaseVerb
    {
        [Value(1, Required = true)]
        public string File { get; set; }
    }
}