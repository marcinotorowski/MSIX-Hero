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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.ViewModel
{
    public class AppAttachViewModel : ChangeableAutomatedDialogViewModel<AppAttachVerb>
    {
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory;

        public AppAttachViewModel(ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory, IInteractionService interactionService) : base("Prepare VHD for app attach", interactionService)
        {
            this.appAttachManagerFactory = appAttachManagerFactory;
            this.interactionService = interactionService;
            this.InputPath = new ChangeableFileProperty("Source MSIX file", interactionService)
            {
                Filter = new DialogFilterBuilder("*.msix").BuildFilter()
            };

            this.GenerateScripts = new ChangeableProperty<bool>(true);

            this.InputPath.Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence };
            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.ExtractCertificate = new ChangeableProperty<bool>();
            this.SizeMode = new ChangeableProperty<AppAttachSizeMode>();
            this.FixedSize = new ValidatedChangeableProperty<string>("Fixed size", "100", this.ValidateFixedSize);
            this.AddChildren(this.InputPath, this.GenerateScripts, this.ExtractCertificate, this.FixedSize, this.SizeMode);

            this.RegisterForCommandLineGeneration(this.InputPath, this.GenerateScripts, this.ExtractCertificate, this.FixedSize, this.SizeMode);
        }

        protected override void UpdateVerbData()
        {
            this.Verb.Package = this.InputPath.CurrentValue;
            this.Verb.CreateScript = this.GenerateScripts.CurrentValue;
            this.Verb.ExtractCertificate = this.ExtractCertificate.CurrentValue;

            if (string.IsNullOrEmpty(this.Verb.Package))
            {
                this.Verb.Package = "<path-to-msix>";
            }

            if (this.SizeMode.CurrentValue == AppAttachSizeMode.Auto)
            {
                this.Verb.Size = 0;
            }
            else
            {
                this.Verb.Size = !uint.TryParse(this.FixedSize.CurrentValue, out var parsed) ? 0 : parsed;
            }

            if (string.IsNullOrEmpty(this.OutputPath) || string.IsNullOrEmpty(Path.GetDirectoryName(this.OutputPath)))
            {
                this.Verb.Directory = "<output-directory>";
            }
            else
            {
                this.Verb.Directory = Path.GetDirectoryName(this.OutputPath);
            }
        }

        public ChangeableFileProperty InputPath { get; }

        public ChangeableProperty<bool> GenerateScripts { get; }

        public ChangeableProperty<bool> ExtractCertificate { get; }
        
        public ValidatedChangeableProperty<string> FixedSize { get; }

        public ChangeableProperty<AppAttachSizeMode> SizeMode { get; }

        public string OutputPath { get; private set; }
        
        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            var settings = new FileDialogSettings("Virtual disks|*.vhd", this.OutputPath);
            if (!this.interactionService.SaveFile(settings, out var output))
            {
                return false;
            }

            this.OutputPath = output;

            var sizeInMegabytes = this.SizeMode.CurrentValue == AppAttachSizeMode.Auto ? 0 : uint.Parse(this.FixedSize.CurrentValue);

            var appAttach = await this.appAttachManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await appAttach.CreateVolume(
                this.InputPath.CurrentValue,
                output,
                sizeInMegabytes,
                this.ExtractCertificate.CurrentValue,
                this.GenerateScripts.CurrentValue,
                cancellationToken,
                progress).ConfigureAwait(false);

            return true;
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                return;
            }

            this.OutputPath = Path.ChangeExtension(this.InputPath.CurrentValue, "vhd");
        }

        private string ValidateFixedSize(string value)
        {
            if (this.SizeMode.CurrentValue == AppAttachSizeMode.Auto)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                return "Fixed size cannot be empty.";
            }

            if (!int.TryParse(value, out var parsed))
            {
                return "Fixed size must ba a number";
            }

            return parsed <= 0 ? "Fixed size must be a positive number" : null;
        }
    }
}

