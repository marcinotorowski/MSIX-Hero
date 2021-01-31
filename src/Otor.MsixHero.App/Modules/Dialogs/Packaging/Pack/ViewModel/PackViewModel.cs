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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Packaging.Packer.Enums;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel
{
    public class PackViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxPacker appxPacker;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private ICommand openSuccessLink;
        private ICommand resetCommand;

        public PackViewModel(
            IAppxPacker appxPacker,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory,
            IConfigurationService configurationService,
            IInteractionService interactionService) : base("Pack MSIX package", interactionService)
        {
            this.appxPacker = appxPacker;
            this.signingManagerFactory = signingManagerFactory;
            var signConfig = configurationService.GetCurrentConfiguration().Signing ?? new SigningConfiguration();
            var signByDefault = configurationService.GetCurrentConfiguration().Packer?.SignByDefault == true;

            this.InputPath = new ChangeableFolderProperty("Source directory", interactionService, ChangeableFolderProperty.ValidatePath);

            this.OutputPath = new ChangeableFileProperty("Target package path", interactionService, ChangeableFileProperty.ValidatePath)
            {
                OpenForSaving = true,
                Filter = new DialogFilterBuilder("*.msix", "*.appx").BuildFilter()
            };

            this.Sign = new ChangeableProperty<bool>(signByDefault);
            this.Compress = new ChangeableProperty<bool>(true);
            this.Validate = new ChangeableProperty<bool>(true);
            this.OverrideSubject = new ChangeableProperty<bool>(true);

            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManagerFactory, signConfig)
            {
                IsValidated = false
            };
            
            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.Sign.ValueChanged += this.SignOnValueChanged;

            this.TabSource = new ChangeableContainer(this.InputPath, this.OutputPath, this.Sign, this.Compress, this.Validate);
            this.TabSigning = new ChangeableContainer(this.SelectedCertificate, this.OverrideSubject);
            this.AddChildren(this.TabSource, this.TabSigning);
        }
        
        public ChangeableContainer TabSource { get; }

        public ChangeableContainer TabSigning { get; }

        public ChangeableFileProperty OutputPath { get; }

        public ChangeableProperty<bool> OverrideSubject { get; }

        public ChangeableFolderProperty InputPath { get; }
        
        public ICommand OpenSuccessLink
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.resetCommand ??= new DelegateCommand(this.ResetExecuted); }
        }

        public CertificateSelectorViewModel SelectedCertificate { get; }

        public ChangeableProperty<bool> Sign { get; }

        public ChangeableProperty<bool> Compress { get; }

        public ChangeableProperty<bool> Validate { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            AppxPackerOptions opts = 0;
            if (!this.Validate.CurrentValue)
            {
                opts |= AppxPackerOptions.NoValidation;
            }

            if (!this.Compress.CurrentValue)
            {
                opts |= AppxPackerOptions.NoCompress;
            }

            using (var progressWrapper = new WrappedProgress(progress))
            {
                var progress1 = progressWrapper.GetChildProgress(50);
                var progress2 = this.Sign.CurrentValue ? progressWrapper.GetChildProgress(30) : null;

                await this.appxPacker.Pack(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, opts, cancellationToken, progress1).ConfigureAwait(false);

                if (this.Sign.CurrentValue)
                {
                    var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);

                    switch (this.SelectedCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Personal:
                            await manager.SignPackageWithInstalled(this.OutputPath.CurrentValue, this.OverrideSubject.CurrentValue, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue?.Model, this.SelectedCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                        case CertificateSource.Pfx:
                            await manager.SignPackageWithPfx(this.OutputPath.CurrentValue, this.OverrideSubject.CurrentValue, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                        case CertificateSource.DeviceGuard:
                            await manager.SignPackageWithDeviceGuardFromUi(this.OutputPath.CurrentValue, this.SelectedCertificate.DeviceGuard.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                    }
                }
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
            Process.Start("explorer.exe", "/select," + this.OutputPath.CurrentValue);
        }

        private void SignOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.SelectedCertificate.IsValidated = this.Sign.CurrentValue;
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                return;
            }

            var newValue = (string) e.NewValue;
            this.OutputPath.CurrentValue = Path.Join(Path.GetDirectoryName(newValue), Path.GetFileName(newValue.TrimEnd('\\'))) + ".msix";
        }
    }
}

