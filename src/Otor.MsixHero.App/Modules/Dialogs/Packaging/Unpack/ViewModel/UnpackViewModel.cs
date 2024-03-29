﻿// MSIX Hero
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.ViewModel
{
    public class UnpackViewModel : ChangeableAutomatedDialogViewModel<UnpackVerb>
    {
        private readonly IAppxPacker appxPacker;
        private ICommand openSuccessLink;
        private ICommand reset;
        public UnpackViewModel(IAppxPacker appxPacker, IInteractionService interactionService) : base("Unpack MSIX package", interactionService)
        {
            this.appxPacker = appxPacker;

            this.OutputPath = new ChangeableFolderProperty("Target directory", interactionService, ChangeableFolderProperty.ValidatePath);
            this.InputPath = new ChangeableFileProperty("Source package path", interactionService, ChangeableFileProperty.ValidatePath)
            {
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension).BuildFilter()
            };
            
            this.CreateFolder = new ChangeableProperty<bool>(true);

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.AddChildren(this.InputPath, this.OutputPath, this.CreateFolder);
            this.RegisterForCommandLineGeneration(this.InputPath, this.OutputPath, this.CreateFolder);
        }

        protected override void UpdateVerbData()
        {
            this.Verb.Directory = this.GetOutputPath();
            this.Verb.Package = this.InputPath.CurrentValue;

            if (string.IsNullOrEmpty(this.Verb.Directory))
            {
                this.Verb.Directory = "<output-directory>";
            }

            if (string.IsNullOrEmpty(this.Verb.Package))
            {
                this.Verb.Package = "<source-package>";
            }
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

            this.OutputPath.CurrentValue = Path.Join(directory, fileName + "_extracted");
        }

        public ChangeableFolderProperty OutputPath { get; }

        public ChangeableFileProperty InputPath { get; }
        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ChangeableProperty<bool> CreateFolder { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            await this.appxPacker.Unpack(this.InputPath.CurrentValue, this.GetOutputPath(), default, progress).ConfigureAwait(false);
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

