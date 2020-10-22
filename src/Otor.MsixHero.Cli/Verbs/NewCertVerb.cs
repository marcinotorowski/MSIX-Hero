using System;
using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("newcert", HelpText = "Create new certificate for self-signing.")]
    public class NewCertVerb
    {
        [Option('n', "name", HelpText = "Certificate name", Required = true)]
        public string DisplayName { get; set; }

        [Option('s', "subject", HelpText = "Certificate subject, for example CN=John. If not provided, it will be set automatically from the display name.")]
        public string Subject { get; set; }

        [Option('d', "directory", HelpText = "Directory, where certificate files will be saved.", Required = true)]
        public string OutputFolder { get; set; }

        [Option("validUntil", HelpText = "Date time until which the certificate can be used for signing purposes. Defaults to one year from today.", Required = false)]
        public DateTime? ValidUntil { get; set; }

        [Option('i', "import", HelpText = "If set, the certificate will be imported to the local store. This option requires that MSIXHeroCLI.exe is started as administrator.")]
        public bool Import { get; set; }

        [Option('p', "password", HelpText = "Certificate password.", Required = false)]
        public string Password { get; set; }
    }
}
