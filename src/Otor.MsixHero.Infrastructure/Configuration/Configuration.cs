using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract(Name = "configuration")]
    public class Configuration : BaseJsonSetting
    {
        public Configuration()
        {
            this.Packages = new PackagesConfiguration();
            this.Events = new EventsConfiguration();
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

        [DataMember(Name = "packages")]
        public PackagesConfiguration Packages { get; set; }

        [DataMember(Name = "events")]
        public EventsConfiguration Events { get; set; }

        [DataMember(Name = "signing")]
        public SigningConfiguration Signing { get; set; }

        [DataMember(Name = "packer")]
        public PackerConfiguration Packer { get; set; }

        [DataMember(Name ="appinstaller")]
        public AppInstallerConfiguration AppInstaller { get; set; }

        [DataMember(Name = "advanced")]
        public AdvancedConfiguration Advanced { get; set; }

        [DataMember(Name = "update")]
        public UpdateConfiguration Update { get; set; }
    }
}
