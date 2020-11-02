using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class UiConfiguration : BaseJsonSetting
    {
        public UiConfiguration()
        {
            this.SwitchToContextTabAfterSelection = true;
            this.ConfirmDeletion = true;
        }

        [DataMember(Name = "switchToContextTabAfterSelection")]
        public bool SwitchToContextTabAfterSelection { get; set; }

        [DataMember(Name = "confirmDeletion")]
        public bool ConfirmDeletion { get; set; }
    }
}
