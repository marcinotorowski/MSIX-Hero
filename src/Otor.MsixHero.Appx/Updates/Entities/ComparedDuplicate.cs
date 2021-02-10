using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparedDuplicate : BaseDuplicateFiles
    {
        public List<ComparedDuplicateFile> Files { get; set; }
    }
}