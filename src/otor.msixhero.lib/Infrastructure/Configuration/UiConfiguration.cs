using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class UiConfiguration : BaseJsonSetting
    {
        public UiConfiguration()
        {
            this.SwitchToContextTabAfterSelection = true;
        }

        public bool SwitchToContextTabAfterSelection { get; set; }
    }
}
