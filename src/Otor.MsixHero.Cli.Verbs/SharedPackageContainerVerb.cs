using System.Collections.Generic;
using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("sharedPackageContainer", HelpText = "CLI_Verbs_SharedPackageContainer_VerbName", ResourceType = typeof(Localization))]
    public class SharedPackageContainerVerb : BaseVerb
    {
        [Option('n', "name", Required = true, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_Name", ResourceType = typeof(Localization))]
        public string Name { get; set; }

        [Option('f', "force", SetName = "deploy", Required = false, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_Force", ResourceType = typeof(Localization))]
        public bool Force { get; set; }

        [Option("forceApplicationShutdown", SetName = "deploy", Required = false, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_ForceApplicationShutdown", ResourceType = typeof(Localization))]
        public bool ForceApplicationShutdown { get; set; }

        [Option('o', "output", SetName = "xml", Required = false, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_Output", ResourceType = typeof(Localization))]
        public string Output { get; set; }

        [Option('m', "merge", SetName = "deploy", Required = false, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_Merge", ResourceType = typeof(Localization))]
        public bool Merge { get; set; }
        
        [Value(1, MetaName = "packages", Required = true, HelpText = "CLI_Verbs_SharedPackageContainer_Prop_Packages", ResourceType = typeof(Localization))]
        public IEnumerable<string> Packages { get; set; }
    }
}
