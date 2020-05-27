using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Version = System.Version;

// https://docs.microsoft.com/en-us/windows/package-manager/package/manifest?tabs=minschema%2Ccompschema
namespace otor.msixhero.lib.Domain.Winget
{
    public class YamlDefinition
    {
        public YamlDefinition()
        {
            this.ManifestVersion = new Version(0, 1, 0);
        }

        [YamlMember(Order = 1)]
        public string Id { get; set; }

        [YamlMember(Order = 2)]
        public string Version { get; set; }

        [YamlMember(Order = 3)]
        public string Name { get; set; }

        [YamlMember(Order = 4)]
        public string Publisher { get; set; }

        [YamlMember(Order = 5)]
        public string License { get; set; }

        [YamlMember(Order = 6)]
        public string LicenseUrl { get; set; }

        [YamlMember(Order = 7)]
        public string AppMoniker { get; set; }

        [YamlMember(Order = 8)]
        public string Commands { get; set; }

        [YamlMember(Order = 9)]
        public string Tags { get; set; }

        [YamlMember(Order = 10)]
        public string Description { get; set; }

        [YamlMember(Order = 11)]
        public string Homepage { get; set; }

        [YamlMember(Order = 12)]
        public IList<YamlInstaller> Installers { get; set; }

        [YamlMember(Order = 13, SerializeAs = typeof(string), Alias = "MinOSVersion")]
        public Version MinOperatingSystemVersion { get; set; }

        [YamlMember(Order = 14)]
        [Obsolete("This property should probably be not used...")]
        public string Language { get; set; }

        [YamlMember(Order = 15, SerializeAs = typeof(string))]
        public Version ManifestVersion { get; set; }

        [YamlMember(Order = 16)]
        [Obsolete("This property should probably be not used...")]
        public string Author { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string FileExtensions { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public YamlLocalization Localization { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Protocols { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Channel { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string InstallLocation { get; set; }

        [YamlMember(Order = int.MaxValue)]
        [Obsolete("This property should probably be not used...")]
        public YamlInstallerType? InstallerType { get; set; }
    }
}
