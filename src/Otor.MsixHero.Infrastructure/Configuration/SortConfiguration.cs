using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class SortConfiguration : BaseJsonSetting
    {
        public SortConfiguration()
        {
            this.SortingMode = PackageSort.Name;
            this.Descending = false;
        }

        [DataMember(Name= "sortingMode")]
        public PackageSort SortingMode { get; set; }

        [DataMember(Name = "descending")]
        public bool Descending { get; set; }
    }
}