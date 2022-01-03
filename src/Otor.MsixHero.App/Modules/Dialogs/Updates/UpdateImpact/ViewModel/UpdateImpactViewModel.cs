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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel
{

    public class UpdateImpactViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxUpdateImpactAnalyzer updateImpactAnalyzer;
        private readonly IInteractionService interactionService;

        public UpdateImpactViewModel(IAppxUpdateImpactAnalyzer updateImpactAnalyzer, IInteractionService interactionService) : base("Analyze update impact", interactionService)
        {
            this.updateImpactAnalyzer = updateImpactAnalyzer;
            this.interactionService = interactionService;
            this.Path1 = new ChangeableFileProperty("Path to the old version", interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                Prompt = "Select the previous version of an upgradable package",
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile).BuildFilter()
            };

            this.Path2 = new ChangeableFileProperty("Path to the new version", interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                Prompt = "Select the newer version of an upgradable package",
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile).BuildFilter()
            };
            
            this.AddChildren(this.Path1, this.Path2);
            this.Compare = new DelegateCommand(this.CompareExecuted);
            this.IsValidated = false;
            this.Export = new DelegateCommand(this.OnExport, this.CanExport);
            this.New = new DelegateCommand(this.OnNew);
            
            this.Path1.ValueChanged += this.Path1OnValueChanged;
            this.Path2.ValueChanged += this.Path2OnValueChanged;
        }

        private void OnExport()
        {
            if (this.Results.CurrentValue == null)
            {
                return;
            }
            
            if (!this.interactionService.SaveFile(FileDialogSettings.FromFilterString("XML files|*.xml"), out var selectedFile))
            {
                return;
            }

            this.Results.CurrentValue.Export(selectedFile);
        }

        private void OnNew()
        {
            if (this.Results.CurrentValue == null)
            {
                return;
            }
            
            this.Results.Reset();
        }

        public ICommand Compare { get; }
        
        public ChangeableFileProperty Path1 { get; }

        public ChangeableFileProperty Path2 { get; }

        public ChangeableProperty<PackageContentDetailsViewModel> OldPackage { get; } = new ChangeableProperty<PackageContentDetailsViewModel>();
        
        public ChangeableProperty<PackageContentDetailsViewModel> NewPackage { get; } = new ChangeableProperty<PackageContentDetailsViewModel>();
        
        public ProgressProperty Progress { get; } = new ProgressProperty();

        public ChangeableProperty<ComparisonViewModel> Results { get; } = new ChangeableProperty<ComparisonViewModel>();
        
        public ICommand New { get; set; }
        
        public ICommand Export { get; set; }
        
        public bool DisableAutoClicks { get; set; }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }

        private async void CompareExecuted()
        {
            await this.ComparePackages();
        }
        
        private async Task ComparePackages(bool ignorePackageVersionError = false) 
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
                using var cts = new CancellationTokenSource();
                var manifestParser = new AppxManifestReader();
                using var file1 = FileReaderFactory.CreateFileReader(this.Path1.CurrentValue);
                using var file2 = FileReaderFactory.CreateFileReader(this.Path2.CurrentValue);

                var task1 = manifestParser.Read(file1, cts.Token);
                var task2 = manifestParser.Read(file2, cts.Token);

                var progress = new Progress<ProgressData>();
                var taskCompare = this.updateImpactAnalyzer.Analyze(this.Path1.CurrentValue, this.Path2.CurrentValue, ignorePackageVersionError, cts.Token, progress);

                var taskComplete = Task.WhenAll(task1, task2, taskCompare);

                this.Progress.MonitorProgress(taskComplete, cts, progress);
                await taskComplete.ConfigureAwait(false);
                var result = await taskCompare.ConfigureAwait(false);

                this.Results.CurrentValue = new ComparisonViewModel(result);
                this.OldPackage.CurrentValue = new PackageContentDetailsViewModel(await task1.ConfigureAwait(false), this.Path1.CurrentValue);
                this.NewPackage.CurrentValue = new PackageContentDetailsViewModel(await task2.ConfigureAwait(false), this.Path2.CurrentValue);
            }
            catch (UpdateImpactException updateImpactException)
            {
                switch (updateImpactException.ErrorType)
                {
                    case UpgradeImpactError.WrongPackageVersion:
                        if (ignorePackageVersionError)
                        {
                            this.interactionService.ShowError(updateImpactException.Message);
                            return;
                        }

                        var result = this.interactionService.ShowError(updateImpactException.Message + "\r\nPress Retry to ignore the version check and compare the packages anyway.", InteractionResult.Retry | InteractionResult.Close, "Invalid versions");
                        if (result == InteractionResult.Retry)
                        {
                            await this.ComparePackages(true).ConfigureAwait(false);
                        }
                        
                        break;
                    default:
                        this.interactionService.ShowError(updateImpactException.Message);
                        return;
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

        private bool CanExport()
        {
            return this.Results.HasValue;
        }

        private void Path1OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.DisableAutoClicks)
            {
                return;
            }    
            
            if (string.IsNullOrEmpty(e.NewValue as string))
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.Path2.CurrentValue))
            {
                return;
            }

            this.Path2.Browse.Execute(null);
        }

        private void Path2OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.DisableAutoClicks)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(e.NewValue as string))
            {
                return;
            }

            if (string.IsNullOrEmpty(this.Path1.CurrentValue))
            {
                return;
            }

            this.Compare.Execute(null);
        }
    }
}

