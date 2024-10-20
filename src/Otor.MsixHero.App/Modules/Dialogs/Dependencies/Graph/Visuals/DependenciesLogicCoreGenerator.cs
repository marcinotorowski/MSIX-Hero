using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Logic.Algorithms.OverlapRemoval;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Dependencies.Domain;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Visuals
{
    public class DependenciesLogicCoreGenerator(IDependencyMapper dependencyMapper)
    {
        public DependencyLogicCore LogicCore { get; private set; }
        
        public AppxPackage Package { get; private set; }

        public async Task GenerateLogic(string packagePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var reader = new AppxManifestReader();
            using (var fileReader = FileReaderFactory.CreateFileReader(packagePath))
            {
                this.Package = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
            }

            var graph = new DependencyBidirectionalGraph();
            var mapping = await dependencyMapper.GetGraph(this.Package, cancellationToken, progress).ConfigureAwait(false);
            var dict = new Dictionary<GraphElement, DependencyVertex>();
            foreach (var item in mapping.Elements)
            {
                DependencyVertex dv;

                if (item is RootGraphElement root)
                {
                    dv = new RootDependencyVertex(root.Package);
                }
                else if (item is PackageGraphElement appxPackage)
                {
                    dv = new InstalledDependencyVertex(appxPackage.Package);
                }
                else if (item is OperatingSystemGraphElement ose)
                {
                    dv = new SystemDependencyVertex(ose.MaxRequiredCaption);
                }
                else
                {
                    dv = new DependencyVertex();

                    if (item is MissingPackageGraphElement mpe)
                    {
                        dv.Text = mpe.PackageName;
                    }
                    else
                    {
                        dv.Text = "?";
                    }
                }

                dv.ID = item.Id;

                graph.AddVertex(dv);
                dict[item] = dv;
            }

            foreach (var relation in mapping.Relations)
            {
                graph.AddEdge(new DependencyEdge()
                {
                    Source = dict[relation.Left],
                    Target = dict[relation.Right],
                    Text = relation.RelationDescription
                });
            }

            var logicCore = new DependencyLogicCore();
            logicCore.Graph = graph;

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.AsyncAlgorithmCompute = false;
            logicCore.EdgeCurvingEnabled = true;

            this.LogicCore = logicCore;
        }
    }
}
