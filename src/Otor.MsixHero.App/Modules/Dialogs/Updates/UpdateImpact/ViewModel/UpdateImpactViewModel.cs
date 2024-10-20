﻿// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Reader;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel
{

    public class UpdateImpactViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxUpdateImpactAnalyzer _updateImpactAnalyzer;
        private readonly IInteractionService _interactionService;

        public UpdateImpactViewModel(IAppxUpdateImpactAnalyzer updateImpactAnalyzer, IInteractionService interactionService) : base(Resources.Localization.Dialogs_UpdateImpact_Title, interactionService)
        {
            MsixHeroTranslation.Instance.CultureChanged += OnCultureChange;
            this._updateImpactAnalyzer = updateImpactAnalyzer;
            this._interactionService = interactionService;
            this.Path1 = new ChangeableFileProperty(() => Resources.Localization.Dialogs_UpdateImpact_File1_Path, interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder().WithPackages().WithManifests().WithAllSupported().WithAll()
            };

            this.Path2 = new ChangeableFileProperty(() => Resources.Localization.Dialogs_UpdateImpact_File2_Path, interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder().WithPackages().WithManifests().WithAll().WithAllSupported()
            };
            
            this.AddChildren(this.Path1, this.Path2);
            this.Compare = new DelegateCommand(this.CompareExecuted);
            this.IsValidated = false;
            this.Export = new DelegateCommand(this.OnExport, this.CanExport);
            this.New = new DelegateCommand(this.OnNew);
            
            this.Path1.ValueChanged += this.Path1OnValueChanged;
            this.Path2.ValueChanged += this.Path2OnValueChanged;

            this.SetLocalizableTexts();
        }
        
        private void OnExport()
        {
            if (this.Results.CurrentValue == null)
            {
                return;
            }
            
            if (!this._interactionService.SaveFile(FileDialogSettings.FromFilterString("XML files|*.xml"), out var selectedFile))
            {
                return;
            }

            this.Results.CurrentValue.Export(selectedFile);
        }

        private void OnCultureChange(object sender, CultureInfo e)
        {
            this.SetLocalizableTexts();
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
        
        private void SetLocalizableTexts()
        {
            this.Path1.Prompt = Resources.Localization.Dialogs_UpdateImpact_File1_HelpText;
            this.Path2.Prompt = Resources.Localization.Dialogs_UpdateImpact_File2_HelpText;
        }

        private async Task ComparePackages(bool ignorePackageVersionError = false) 
        { 
            this.IsValidated = true;

            if (!this.IsValid)
            {
                this._interactionService.ShowError(this.ValidationMessage, InteractionResult.OK, Resources.Localization.Dialogs_UpdateImpact_Errors_MissingValues);
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
                var taskCompare = this._updateImpactAnalyzer.Analyze(this.Path1.CurrentValue, this.Path2.CurrentValue, ignorePackageVersionError, cts.Token, progress);

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
                            this._interactionService.ShowError(updateImpactException.Message);
                            return;
                        }

                        var result = this._interactionService.ShowError(updateImpactException.Message + "\r\n" + Resources.Localization.Dialogs_UpdateImpact_Errors_IgnoreRetry, InteractionResult.Retry | InteractionResult.Close, Resources.Localization.Dialogs_UpdateImpact_Errors_InvalidVersions);
                        if (result == InteractionResult.Retry) 
                        {
                            await this.ComparePackages(true).ConfigureAwait(false);
                        }
                        
                        break;
                    default:
                        this._interactionService.ShowError(updateImpactException.Message);
                        return;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(Resources.Localization.Dialogs_UpdateImpact_Errors + " " + e.Message, e);
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

