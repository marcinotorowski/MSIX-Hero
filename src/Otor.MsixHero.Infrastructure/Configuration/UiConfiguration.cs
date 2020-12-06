using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public enum DefaultScreen
    {
        Dashboard = 0,
        Packages = 1,
        Volumes = 2,
        Events = 3,
        System = 4
    }

    [DataContract]
    public class UiConfiguration : BaseJsonSetting
    {
        public UiConfiguration()
        {
            this.ConfirmDeletion = true;
            this.DefaultScreen = DefaultScreen.Packages;
        }

        [DataMember(Name = "confirmDeletion")]
        public bool ConfirmDeletion { get; set; }

        [DataMember(Name = "defaultScreen")]
        public DefaultScreen DefaultScreen { get; set; }
    }
}
