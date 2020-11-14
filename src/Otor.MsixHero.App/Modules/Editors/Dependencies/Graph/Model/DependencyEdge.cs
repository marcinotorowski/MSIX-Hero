using GraphX.Common.Models;

namespace Otor.MsixHero.App.Modules.Editors.Dependencies.Graph.Model
{
    public class DependencyEdge : EdgeBase<DependencyVertex>
    {
        public DependencyEdge(DependencyVertex source, DependencyVertex target, double weight = 1) : base(source, target, weight)
        {
        }

        public DependencyEdge() : base(null, null, 1)
        {
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}