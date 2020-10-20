using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphX.Common.Enums;
using GraphX.Common.Interfaces;
using GraphX.Controls;
using GraphX.Controls.Models;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Logic.Algorithms.OverlapRemoval;
using Otor.Msix.Dependencies;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Services;
using QuickGraph;
using Point = GraphX.Measure.Point;
using Size = GraphX.Measure.Size;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class ExampleExternalLayoutAlgorithm : IExternalLayout<DataVertex, DataEdge>
        {
            public bool SupportsObjectFreeze
            {
                get { return true; }
            }

            public void ResetGraph(IEnumerable<DataVertex> vertices, IEnumerable<DataEdge> edges)
            {
                _graph = default(IMutableBidirectionalGraph<DataVertex, DataEdge>);
                _graph.AddVertexRange(vertices);
                _graph.AddEdgeRange(edges);
            }

            private IMutableBidirectionalGraph<DataVertex, DataEdge> _graph;
            public ExampleExternalLayoutAlgorithm(IMutableBidirectionalGraph<DataVertex, DataEdge> graph)
            {
                _graph = graph;
            }

            public void Compute(CancellationToken cancellationToken)
            {
                var pars = new EfficientSugiyamaLayoutParameters { LayerDistance = 200 };
                var algo = new EfficientSugiyamaLayoutAlgorithm<DataVertex, DataEdge, IMutableBidirectionalGraph<DataVertex, DataEdge>>(_graph, pars, VertexPositions, VertexSizes);
                algo.Compute(cancellationToken);

                // now you can use = algo.VertexPositions for custom manipulations

                //set this algo calculation results 
                VertexPositions = algo.VertexPositions;
            }

            public IDictionary<DataVertex, Point> VertexPositions { get; set; }

            public IDictionary<DataVertex, Size> VertexSizes { get; set; }

            public bool NeedVertexSizes
            {
                get { return true; }
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            Random Rand = new Random();

            //Create data graph object
            var graph = new GraphExample();
            this.gg_Area.EdgeSelected += Gg_AreaOnEdgeSelected;

            var fie = @"C:\Program Files\WindowsApps\Microsoft.Photos.MediaEngineDLC_1.0.0.0_x64__8wekyb3d8bbwe\AppxManifest.xml";
            var dm = new DependencyMapper(new AppxPackageManager(new RegistryManager(), new LocalConfigurationService()));

            var mapping = dm.GetGraph(fie).GetAwaiter().GetResult();

            var dict = new Dictionary<GraphElement, DataVertex>();
            
            foreach (var item in mapping.Elements)
            {
                var dv = new DataVertex
                {
                    ID = item.Id
                };

                if (item is InstalledPackageElement ipe)
                {
                    dv.Text = ipe.Package.DisplayName + System.Environment.NewLine + ipe.Package.Version;
                    dv.Shape = VertexShape.Diamond;
                }
                else if (item is MissingPackageElement mpe)
                {
                    dv.Text = mpe.PackageName;
                    dv.Shape = VertexShape.Diamond;
                }
                else if (item is OperatingSystemElement ose)
                {
                    dv.Text = ose.OperatingSystem;
                    dv.Shape = VertexShape.Rectangle;
                }
                else if (item is Root re)
                {
                    dv.Text = re.Package.DisplayName + Environment.NewLine + re.Package.Version;
                    dv.Shape = VertexShape.Circle;
                }
                else
                {
                    dv.Text = "?";
                    dv.Shape = VertexShape.Diamond;
                }

                graph.AddVertex(dv);
                dict[item] = dv;
            }

            foreach (var relation in mapping.Relations)
            {
                graph.AddEdge(new DataEdge()
                {
                    Source = dict[relation.Left],
                    Target = dict[relation.Right],
                    Text = relation.RelationDescription
                });
            }

            /*foreach (var item in Enumerable.Range(0, 9))
            {
                var dv = new DataVertex() {ID = item, Text = "Item " + item};
                var rnd = Rand.Next(4);
                if (rnd > 2)
                {
                    dv.GroupId = rnd;
                }
                graph.AddVertex(dv);
            }

            var vlist = graph.Vertices.ToList();
            //Generate random edges for the vertices
            foreach (var item in vlist)
            {
                do
                {
                    var vertex2 = vlist[Rand.Next(0, graph.VertexCount - 1)];
                    graph.AddEdge(new DataEdge(
                        item,
                        vertex2,
                        Rand.Next(1, 50))
                    {
                        Text = string.Format("{0} -> {1}", item, vertex2),
                        Weight = Rand.NextDouble() + 0.1
                    });
                } while (Rand.Next(0, 50) < 35);
            }*/

            var logicCore = new GXLogicCoreExample();
            logicCore.Graph = graph;

            //Different algorithms uses different values and some of them uses edge Weight property.
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            //Now we can set optional parameters using AlgorithmFactory
            //NOTE: default parameters can be automatically created each time you change Default algorithms
            logicCore.DefaultLayoutAlgorithmParams =
                logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            //Setup optional params
            logicCore.DefaultOverlapRemovalAlgorithmParams =
                logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            logicCore.EdgeCurvingEnabled = true;
            

            this.gg_Area.LogicCore = logicCore;
            logicCore.ExternalLayoutAlgorithm = new ExampleExternalLayoutAlgorithm(graph);
            this.gg_Area.GenerateGraph(true);
        }

        private void Gg_AreaOnEdgeSelected(object sender, EdgeSelectedEventArgs args)
        {
            DragBehaviour.SetIsTagged(args.EdgeControl, true);
        }
    }
}
