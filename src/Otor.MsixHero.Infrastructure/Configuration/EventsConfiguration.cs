using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class EventsConfiguration : BaseJsonSetting
    {
        public EventsConfiguration()
        {
            this.Filter = new EventsFilterConfiguration();
            this.Sorting = new EventsSortConfiguration();
        }

        [DataMember(Name = "filter")]
        public EventsFilterConfiguration Filter { get; set; }
        
        [DataMember(Name = "sorting")]
        public EventsSortConfiguration Sorting { get; set; }
    }
}