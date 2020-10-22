﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Logic.Algorithms.OverlapRemoval;
using Otor.Msix.Dependencies;
using Otor.Msix.Dependencies.Domain;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Controls.Progress;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.Model;
using Prism.Services.Dialogs;
using DelegateCommand = Otor.MsixHero.Ui.Commands.RoutedCommand.DelegateCommand;

namespace Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.ViewModel
{

    public class DependencyViewerViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IDependencyMapper dependencyMapper;
        private readonly IInteractionService interactionService;
        private bool isLoaded;
        private bool showSelector = true;
        private string tileColor;
        private AppxPackage package;

        public DependencyViewerViewModel(
            IInteractionService interactionService, 
            IDependencyMapper dependencyMapper) : base("Analyze dependencies", interactionService)
        {
            this.dependencyMapper = dependencyMapper;
            this.interactionService = interactionService;
            this.Path = new ChangeableFileProperty("Path to the package", interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                Filter = "MSIX/APPX packages|*.msix;*.appx|Manifest files|AppxManifest.xml|All files|*.*"
            };

            this.AddChildren(this.Path);
            this.Analyze = new DelegateCommand(this.AnalyzeExecuted);
            this.IsValidated = false;

            this.Export = new DelegateCommand(this.OnExport);
            this.Print = new DelegateCommand(this.OnPrint);
        }

        public ICommand Export { get; }

        public ICommand Print { get; }

        public ICommand Analyze { get; }

        public ChangeableFileProperty Path { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();
        
        public bool IsLoaded
        {
            get => this.isLoaded;
            private set => this.SetField(ref this.isLoaded, value);
        }

        public bool ShowSelector
        {
            get => this.showSelector;
            private set => this.SetField(ref this.showSelector, value);
        }

        public string TileColor
        {
            get => this.tileColor;
            private set => this.SetField(ref this.tileColor, value);
        }

        public AppxPackage Package
        {
            get => this.package;
            set => this.SetField(ref this.package, value);
        }

        public IExporterHandler ExporterHandler { get; set; }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var par = parameters.Keys.FirstOrDefault();
            if (par == null)
            {
                return;
            }

            this.ShowSelector = false;
            this.Path.CurrentValue = parameters.GetValue<string>(par);
            this.Analyze.Execute(null);
        }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }

        public event EventHandler<DependencyLogicCore> LogicLoaded;
        
        private async void AnalyzeExecuted(object obj)
        {
            this.IsValidated = true;

            if (!this.IsValid)
            {
                this.interactionService.ShowError(this.ValidationMessage, InteractionResult.OK, "Missing values");
                return;
            }

            this.Progress.Progress = -1;
            this.Progress.IsLoading = true;
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    var analyzer = new LogicCoreGenerator(this.dependencyMapper);

                    var progress = new Progress<ProgressData>();
                    var task = analyzer.GenerateLogic(this.Path.CurrentValue, cts.Token, progress);
                    var minimum = Task.Delay(1100, cts.Token);

                    var combinedTask = Task.WhenAll(task, minimum);

                    this.Progress.MonitorProgress(combinedTask, cts, progress);

                    await task.ConfigureAwait(true);

                    this.Package = analyzer.Package;
                    this.TileColor = this.Package.Applications.Select(a => a.BackgroundColor).FirstOrDefault(a => !string.IsNullOrEmpty(a));
                    this.ShowSelector = false;

                    this.LogicLoaded?.Invoke(this, analyzer.LogicCore);
                    await combinedTask.ConfigureAwait(false);
                    this.IsLoaded = true;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                this.interactionService.ShowError("Could not compare selected packages. " + e.Message, e);
            }
            finally
            {
                this.Progress.IsLoading = false;
            }
        }
        
        private void OnExport(object obj)
        {
            if (obj is string extension)
            {
                switch (extension.ToLowerInvariant())
                {
                    case ".png":
                        this.ExporterHandler?.ExportPng();
                        break;
                    case ".jpg":
                        this.ExporterHandler?.ExportJpg();
                        break;
                    default:
                        this.ExporterHandler?.ExportPng();
                        break;
                }
            }
            else
            {
                this.ExporterHandler?.ExportPng();
            }
        }
        private void OnPrint(object obj)
        {
            this.ExporterHandler?.Print();
        }

        public interface IExporterHandler
        {
            void ExportPng();

            void ExportJpg();

            void Print();
        }

        private class LogicCoreGenerator
        {
            private readonly IDependencyMapper dependencyMapper;

            public LogicCoreGenerator(IDependencyMapper dependencyMapper)
            {
                this.dependencyMapper = dependencyMapper;
            }

            public DependencyLogicCore LogicCore { get; private set; }

            public AppxPackage Package { get; private set; }

            public async Task GenerateLogic(string packagePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
            {
                var reader = new AppxManifestReader();
                if (string.Equals("appxmanifest.xml", System.IO.Path.GetFileName(packagePath), StringComparison.OrdinalIgnoreCase))
                {
                    using (IAppxFileReader fileReader = new FileInfoFileReaderAdapter(packagePath))
                    {
                        this.Package = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(packagePath))
                    {
                        this.Package = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
                    }
                }
                
                var graph = new Model.DependencyGraph();
                var mapping = await this.dependencyMapper.GetGraph(this.Package, cancellationToken, progress).ConfigureAwait(false);
                var dict = new Dictionary<GraphElement, DependencyVertex>();
                foreach (var item in mapping.Elements)
                {
                    DependencyVertex dv;

                    if (item is InstalledPackageGraphElement installed)
                    {
                        dv = new PackageDependencyVertex(installed);
                    }
                    else if (item is RootGraphElement root)
                    {
                        dv = new RootDependencyVertex(root.Package);
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
}