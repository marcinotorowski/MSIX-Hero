using System.Collections.Generic;
using CommandLine;
using Otor.MsixHero.Appx.Signing;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("sign", HelpText = "Sign a package")]
    public class SignVerb
    {
        [Value(1, MetaName = "file path", HelpText = "Full paths to one or more files (separated by space).")]
        public IEnumerable<string> FilePath { get; set; }

        [Option("sm", HelpText = "Specifies that a machine store, instead of a user store, is used.", Default = false, Required = false)]
        public bool UseMachineStore { get; set; }

        [Option('t', "timestamp", HelpText = "Specifies the URL of the RFC 3161 time stamp server. If not specified, the default value from MSIX Hero settings will be used.", Required = false)]
        public string TimeStampUrl { get; set; }
        
        [Option("sha1", HelpText = "Specifies the SHA1 hash of the signing certificate. The SHA1 hash is commonly specified when multiple certificates satisfy the criteria specified by the remaining switches.", Required = false)]
        public string ThumbPrint { get; set; }

        [Option('f', "file", HelpText = "Specifies the signing certificate in a file. If the file is in Personal Information Exchange (PFX) format and protected by a password, use the -p option to specify the password.", Required = false)]
        public string PfxFilePath { get; set; }
        
        [Option('p', "password", HelpText = "	Specifies the password to use when opening a PFX file.", Required = false)]
        public string PfxPassword { get; set; }

        [Option('i', "increaseVersion", HelpText = "Specifies whether the version should be automatically increased, and (if yes) which component of it. Supported values are [None, Major, Minor, Build, Revision].", Default = IncreaseVersionMethod.None, Required = false)]
        public IncreaseVersionMethod IncreaseVersion { get; set; }
    }
}
