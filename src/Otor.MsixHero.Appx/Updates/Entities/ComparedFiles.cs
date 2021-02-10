using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparedFiles : BaseCompared
    {
        public IList<ComparedFile> Files { get; set; }
    }
}