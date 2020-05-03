using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract(Name = "configuration")]
    public class Configuration : BaseJsonSetting
    {
        public Configuration()
        {
            this.List = new ListConfiguration();
            this.Signing = new SigningConfiguration();
            this.Packer = new PackerConfiguration();
            this.AppInstaller = new AppInstallerConfiguration();
            this.Editing = new EditingConfiguration();
            this.UiConfiguration = new UiConfiguration();
        }

        [DataMember(Name = "editing")]
        public EditingConfiguration Editing { get; set; }

        [DataMember(Name = "ui")]
        public UiConfiguration UiConfiguration { get; set; }

        [DataMember(Name = "list")]
        public ListConfiguration List { get; set; }

        [DataMember(Name = "signing")]
        public SigningConfiguration Signing { get; set; }

        [DataMember(Name = "packer")]
        public PackerConfiguration Packer { get; set; }

        [DataMember(Name ="appinstaller")]
        public AppInstallerConfiguration AppInstaller { get; set; }
    }
}
