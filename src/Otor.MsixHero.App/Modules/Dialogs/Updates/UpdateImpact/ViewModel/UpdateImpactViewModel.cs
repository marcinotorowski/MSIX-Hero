// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Updates;
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
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*.msix", "*.appx", "appxmanifest.xml").BuildFilter()
            };

            this.Path2 = new ChangeableFileProperty("Path to the new version", interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = true,
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*.msix", "*.appx", "appxmanifest.xml").BuildFilter()
            };
            
            this.AddChildren(this.Path1, this.Path2);
            this.Compare = new DelegateCommand(this.CompareExecuted);
            this.IsValidated = false;
        }

        public ICommand Compare { get; }

        public ChangeableFileProperty Path1 { get; }

        public ChangeableFileProperty Path2 { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public AsyncProperty<ComparisonViewModel> Results { get; } = new AsyncProperty<ComparisonViewModel>();

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }

        private async void CompareExecuted()
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
                    var task = this.updateImpactAnalyzer.Analyze(this.Path1.CurrentValue, this.Path2.CurrentValue, cts.Token);
                    this.Progress.MonitorProgress(task, cts);
                    var result = await task.ConfigureAwait(false);
                    await this.Results.Load(Task.FromResult(new ComparisonViewModel(result))).ConfigureAwait(false);
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
    }
}

