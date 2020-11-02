using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    public class YamlSwitches
    {
        [YamlMember(Order = 1)]
        public string Silent { get; set; }

        [YamlMember(Order = 2)]
        public string SilentWithProgress { get; set; }

        [YamlMember(Order = 3)]
        public string Custom { get; set; }

        [YamlMember(Order = 4)]
        public string Language { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Interactive { get; set; }
    }
}