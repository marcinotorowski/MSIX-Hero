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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Winget.Yaml;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Winget.YamlEditor.ViewModel
{
    public class WingetViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly WingetValidateWrapper yamlValidator = new WingetValidateWrapper();
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink;
        private ICommand reset;
        private ICommand open;
        private string yamlPath;

        public WingetViewModel(IInteractionService interactionService) : base(Resources.Localization.Dialogs_Winget_Title, interactionService)
        {
            this.interactionService = interactionService;
            this.AddChild(this.Definition = new WingetDefinitionViewModel(interactionService));
        }

        public WingetDefinitionViewModel Definition { get; private set; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ICommand OpenCommand
        {
            get { return this.open ??= new DelegateCommand(this.OpenExecuted, this.CanOpen); }
        }

        public bool WingetVerified { get; private set; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            this.WingetVerified = false;

            if (!this.IsValid)
            {
                return false;
            }

            string selected;

            FileDialogSettings settings;
            if (string.IsNullOrEmpty(this.yamlPath) || !File.Exists(this.yamlPath))
            {
                settings = FileDialogSettings.FromFilterString(new DialogFilterBuilder("*" + FileConstants.WingetExtension).BuildFilter());
            }    
            else
            {
                settings = new FileDialogSettings(new DialogFilterBuilder("*" + FileConstants.WingetExtension).BuildFilter(), null, this.yamlPath);
            }
            
            var userSelected = this.interactionService.SaveFile(settings, out selected);

            if (!userSelected)
            {
                return false;
            }

            this.yamlPath = selected;

            var tempPath = Path.Combine(Path.GetTempPath(), "msixhero-" + Guid.NewGuid().ToString("N").Substring(0, 8) + FileConstants.WingetExtension);
            try
            {
                if (!(await this.Definition.Save(tempPath, cancellationToken, progress).ConfigureAwait(false)))
                {
                    return false;
                }

                var winGetPath = await this.yamlValidator.GetWingetPath(cancellationToken).ConfigureAwait(false);
                if (winGetPath != null)
                {
                    progress.Report(new ProgressData(100, Resources.Localization.Dialogs_Winget_ValidatingCli));

                    var validationDetails = await yamlValidator.ValidateAsync(tempPath, false, cancellationToken).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken).ConfigureAwait(false);
                    
                    if (validationDetails != null)
                    {
                        if (1 == this.interactionService.ShowMessage(Resources.Localization.Dialogs_Winget_ValidatingCli_Body, 
                                new[] { Resources.Localization.Dialogs_Winget_ValidatingCli_Ignore, Resources.Localization.Dialogs_Winget_ValidatingCli_Fix }, 
                                Resources.Localization.Dialogs_Winget_ValidatingCli_Title, validationDetails))
                        {
                            this.WingetVerified = false;
                            this.OnPropertyChanged(nameof(WingetVerified));
                            return false;
                        }
                    }
                    else
                    {
                        this.WingetVerified = true;
                        this.OnPropertyChanged(nameof(WingetVerified));
                    }
                }
                
                File.Move(tempPath, selected, true);
                return true;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        public ProgressProperty GeneralProgress { get; } = new ProgressProperty();

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("yaml", out string yamlSelectedPath))
            {
                this.yamlPath = Path.ChangeExtension(yamlSelectedPath, FileConstants.WingetExtension);
#pragma warning disable 4014
                this.GeneralProgress.MonitorProgress(this.Definition.LoadFromYaml(yamlSelectedPath));
            }
            else if (parameters.TryGetValue("msix", out string msixPath))
            {
                this.yamlPath = Path.ChangeExtension(Path.GetFileNameWithoutExtension(msixPath), FileConstants.WingetExtension);
                this.GeneralProgress.MonitorProgress(this.Definition.LoadFromFile(msixPath));
            }
            else
            {
                this.GeneralProgress.MonitorProgress(this.Definition.NewManifest(CancellationToken.None));
            }
        }

        private void ResetExecuted()
        {
            this.State.IsSaved = false;
        }

        private bool CanOpen()
        {
            return !this.State.IsSaved;
        }

        private async void OpenExecuted()
        {
            if (this.State.IsSaved)
            {
                this.State.IsSaved = false;
            }

            if (!this.interactionService.SelectFile(FileDialogSettings.FromFilterString(new DialogFilterBuilder("*" + FileConstants.WingetExtension).BuildFilter()), out var selectedFile))
            {
                return;
            }

            this.yamlPath = selectedFile;
            var task = this.Definition.LoadFromYaml(selectedFile);
            this.GeneralProgress.MonitorProgress(task);
            await task.ConfigureAwait(false);
        }

        private void OpenSuccessLinkExecuted()
        {
            Process.Start("explorer.exe", "/select," + this.yamlPath);
        }
    }
}

