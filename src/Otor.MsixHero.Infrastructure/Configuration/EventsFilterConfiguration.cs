using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class EventsFilterConfiguration : BaseJsonSetting
    {
        public EventsFilterConfiguration()
        {
            this.Filter = EventFilter.Default;
        }

        [DataMember(Name="filter")]
        public EventFilter Filter { get; set; }
    }
}