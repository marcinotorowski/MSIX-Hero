using GraphX.Logic.Models;
using QuickGraph;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Model
{
    public class DependencyLogicCore : GXLogicCore<DependencyVertex, DependencyEdge, BidirectionalGraph<DependencyVertex, DependencyEdge>> { }
}