using System.Collections.Generic;

namespace Otor.Msix.Dependencies.Domain
{
    public class DependencyGraph
    {
        public IList<GraphElement> Elements { get; set; }

        public IList<Relation> Relations { get; set; }
    }
}