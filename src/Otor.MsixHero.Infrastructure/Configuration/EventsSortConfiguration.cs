using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class EventsSortConfiguration : BaseJsonSetting
    {
        public EventsSortConfiguration()
        {
            this.SortingMode = EventSort.Date;
            this.Descending = true;
        }

        [DataMember(Name= "sortingMode")]
        public EventSort SortingMode { get; set; }

        [DataMember(Name = "descending")]
        public bool Descending { get; set; }
    }
}