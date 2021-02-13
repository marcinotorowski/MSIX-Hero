using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("updateImpact", HelpText = "Analyzes the difference between two packages and predicts the impact on updates.")]
    public class UpdateImpactVerb
    {
        [Option('o', "oldPackagePath", Required = true, HelpText = "The older version of the package to compare. Accepted files: *.msix, *.appx, AppxManifest.xml and AppxBlockMap.xml.")]
        public string OldPackagePath { get; set; }

        [Option('n', "newPackagePath", Required = true, HelpText = "The newer version of the package to compare. Accepted files: *.msix, *.appx, AppxManifest.xml and AppxBlockMap.xml.")]
        public string NewPackagePath { get; set; }

        [Option('f', "force", HelpText = "Forces the comparison even if the 'newVersion' is not higher than the 'oldVersion'.")]
        public bool IgnoreVersionMismatch { get; set; }

        [Option('x', "xml", HelpText = "Full file path where the XML file with detailed comparison will be saved.")]
        public string OutputXml { get; set; }
    }
}
