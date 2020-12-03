using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("newmodpack", HelpText = "Create a new modification package.")]
    public class NewModPackVerb : BaseVerb
    {
        [Value(0, HelpText = "Path to the output file. If the specified value has extension .msix, a packaged package will be created. Otherwise, the value is treated as a folder where unpacked resources will be saved.", Required = true)]
        public string OutputPath { get; set; }

        [Option("name", HelpText = "The displayed name of the new modification package.", Required = true)]
        public string Name { get; set; }

        [Option("displayName", HelpText = "The displayed name of the new modification package.", Required = true)]
        public string DisplayName { get; set; }

        [Option("publisherName", HelpText = "The publisher name of the new modification package.", Required = true)]
        public string PublisherName { get; set; }

        [Option("publisherDisplayName", HelpText = "The displayed name of the publisher of the new modification package.", Required = true)]
        public string PublisherDisplayName { get; set; }

        [Option("version", HelpText = "The version of the new modification package.", Required = true)]
        public string Version { get; set; }

        [Option("parentPath", HelpText = "Full path to the parent package (a manifest or .msix package).", SetName = "Parent is an MSIX package", Required = true)]
        public string ParentPackagePath { get; set; }

        [Option("parentName", HelpText = "The name of the parent package.", SetName = "Parent from arbitrary meta-data.", Required = true)]
        public string ParentName { get; set; }

        [Option("parentPublisherName", HelpText = "The name of the parent package publisher.", SetName = "Parent from arbitrary meta-data.", Required = true)]
        public string ParentPublisher { get; set; }

        [Option('r', "registry", HelpText = "Path to a .REG file containing registry keys to be inserted into the new modification package.")]
        public string IncludeRegFile { get; set; }

        [Option('f', "folder", HelpText = "Path to a folder that will be inserted into the root folder of the new modification package.")]
        public string IncludeFolder { get; set; }

        [Option('c', "copyFolderStructure", HelpText = "Indicates whether to copy the folder structure from the parent package. This option is only valid if --parentPackage parameter is defined AND the target file path is a folder.")]
        public bool CopyFolderStructure { get; set; }
    }
}
