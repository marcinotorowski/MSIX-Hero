using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparedDuplicateFiles : BaseDuplicateFiles
    {
        public List<ComparedDuplicate> Duplicates { get; set; }
    }
}