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
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.ViewModel
{
    public class UnpackViewModel : ChangeableAutomatedDialogViewModel<UnpackVerb>
    {
        private readonly IAppxPacker _appxPacker;
        private ICommand _openSuccessLink;
        private ICommand _reset;
        public UnpackViewModel(IAppxPacker appxPacker, IInteractionService interactionService) : base(Resources.Localization.Dialogs_Unpack_Title, interactionService)
        {
            this._appxPacker = appxPacker;

            this.OutputPath = new ChangeableFolderProperty(() => Resources.Localization.Dialogs_Unpack_TargetDir, interactionService, ChangeableFolderProperty.ValidatePath);
            this.InputPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Unpack_SourceMsix, interactionService, ChangeableFileProperty.ValidatePath)
            {
                Filter = new DialogFilterBuilder().WithPackages().WithAll()
            };
            
            this.CreateFolder = new ChangeableProperty<bool>(true);
            this.RemoveFile = new ChangeableProperty<bool>();

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.AddChildren(this.InputPath, this.OutputPath, this.CreateFolder, this.RemoveFile);

            this.RegisterForCommandLineGeneration(this.InputPath, this.OutputPath, this.CreateFolder, this.RemoveFile);
        }

        protected override void UpdateVerbData()
        {
            this.Verb.Directory = this.GetOutputPath();
            this.Verb.Package = this.InputPath.CurrentValue;

            if (string.IsNullOrEmpty(this.Verb.Directory))
            {
                this.Verb.Directory = Resources.Localization.Dialogs_Unpack_Cmd_Placeholder_OutputDir;
            }

            if (string.IsNullOrEmpty(this.Verb.Package))
            {
                this.Verb.Package = Resources.Localization.Dialogs_Unpack_Cmd_Placeholder_SourcePkg;
            }

            this.Verb.RemovePackageAfterExtraction = this.RemoveFile.CurrentValue;
        }
        
        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                return;
            }

            var newFilePath = new FileInfo((string) e.NewValue);

            // ReSharper disable once PossibleNullReferenceException
            var directory = newFilePath.Directory.FullName;

            var fileName = newFilePath.Name;

            this.OutputPath.CurrentValue = Path.Join(directory, fileName + Resources.Localization.Dialogs_Unpack_TargetDir_Suffix);
        }

        public ChangeableFolderProperty OutputPath { get; }

        public ChangeableFileProperty InputPath { get; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this._openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this._reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ChangeableProperty<bool> CreateFolder { get; }

        public ChangeableProperty<bool> RemoveFile { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            await this._appxPacker.Unpack(this.InputPath.CurrentValue, this.GetOutputPath(), default, progress).ConfigureAwait(false);

            if (this.RemoveFile.CurrentValue)
            {
                ExceptionGuard.Guard(() => File.Delete(this.InputPath.CurrentValue));
            }

            return true;
        }

        private void ResetExecuted()
        {
            this.InputPath.Reset();
            this.OutputPath.Reset();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted()
        { 
            Process.Start("explorer.exe", this.GetOutputPath());
        }

        private string GetOutputPath()
        {
            if (!string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return this.CreateFolder.CurrentValue ? Path.Combine(this.OutputPath.CurrentValue, Path.GetFileNameWithoutExtension(this.InputPath.CurrentValue)) : this.OutputPath.CurrentValue;
            }

            return null;
        }
    }
}

