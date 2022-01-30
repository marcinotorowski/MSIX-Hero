// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Visuals;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.ViewModel
{

    public class DependencyViewerViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IDependencyMapper _dependencyMapper;
        private readonly IInteractionService _interactionService;
        private bool _isLoaded;
        private bool _showSelector = true;
        private string _tileColor;
        private AppxPackage _package;

        public DependencyViewerViewModel(
            IInteractionService interactionService, 
            IDependencyMapper dependencyMapper) : base(Resources.Localization.Dialogs_Dependencies_Title, interactionService)
        {
            this._dependencyMapper = dependencyMapper;
            this._interactionService = interactionService;
            this.Path = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Dependencies_SelectedPackage, interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile).BuildFilter()
            };

            this.AddChildren(this.Path);
            this.Analyze = new DelegateCommand(this.AnalyzeExecuted);
            this.IsValidated = false;

            this.Export = new DelegateCommand<string>(this.OnExport);
            this.Print = new DelegateCommand(this.OnPrint);
        }

        public ICommand Export { get; }

        public ICommand Print { get; }

        public ICommand Analyze { get; }

        public ChangeableFileProperty Path { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();
        
        public bool IsLoaded
        {
            get => this._isLoaded;
            private set => this.SetField(ref this._isLoaded, value);
        }

        public bool ShowSelector
        {
            get => this._showSelector;
            private set => this.SetField(ref this._showSelector, value);
        }

        public string TileColor
        {
            get => this._tileColor;
            private set => this.SetField(ref this._tileColor, value);
        }

        public AppxPackage Package
        {
            get => this._package;
            set => this.SetField(ref this._package, value);
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
        
        private async void AnalyzeExecuted()
        {
            this.IsValidated = true;

            if (!this.IsValid)
            {
                this._interactionService.ShowError(this.ValidationMessage, InteractionResult.OK, Resources.Localization.Dialogs_Dependencies_Validation_Missing);
                return;
            }

            this.Progress.Progress = -1;
            this.Progress.IsLoading = true;
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    var analyzer = new DependenciesLogicCoreGenerator(this._dependencyMapper);

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
                this._interactionService.ShowError(Resources.Localization.Dialogs_Dependencies_Error_Failed + " " + e.Message, e);
            }
            finally
            {
                this.Progress.IsLoading = false;
            }
        }
        
        private void OnExport(string extension)
        {
            if (!string.IsNullOrEmpty(extension))
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
                this.ExporterHandler?.ExportClipboard();
            }
        }
        private void OnPrint()
        {
            this.ExporterHandler?.Print();
        }

        public interface IExporterHandler
        {
            void ExportPng();

            void ExportJpg();

            void ExportClipboard();

            void Print();
        }
    }
}