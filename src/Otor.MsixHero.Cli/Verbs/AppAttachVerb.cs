using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("appattach", HelpText = "Creates VHD disk fro app attach.")]
    public class AppAttachVerb
    {
        [Option('p', "package", HelpText = "Full path to MSIX package to be converted to VHD.", Required = true)]
        public string Package { get; set; }

        [Option('d', "directory", HelpText = "Output directory folder.", Required = true)]
        public string Directory { get; set; }

        [Option('n', "name", HelpText = "File name for VHD output. If left empty, the MSIX file name without extension will be used.", Required = false)]
        public string Name { get; set; }
        
        [Option('s', "size", HelpText = "Size of VHD container (in MB). Leave empty for auto-selection.", Required = false)]
        public uint Size { get; set; }

        [Option('c', "createScripts", HelpText = "If specified, sample PS1 scripts for registration and de-registration will be created.", Required = false)]
        public bool CreateScript { get; set; }

        [Option('e', "extractCert", HelpText = "If specified, digital certificate will be extracted from the specified package.", Required = false)]
        public bool ExtractCertificate { get; set; }
    }
}
