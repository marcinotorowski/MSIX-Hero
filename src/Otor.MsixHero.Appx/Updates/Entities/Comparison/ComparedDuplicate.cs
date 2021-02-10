using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public class ComparedDuplicate : BaseDuplicateFiles
    {
        public List<ComparedDuplicateFile> Files { get; set; }
    }
}