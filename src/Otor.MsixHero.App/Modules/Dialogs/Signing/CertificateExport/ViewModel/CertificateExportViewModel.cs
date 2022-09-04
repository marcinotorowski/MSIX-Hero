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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Modules.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.ViewModel
{
    public enum CertOperationType
    {
        Import,
        Extract,
    }

    public class CertificateExportViewModel : ChangeableAutomatedDialogViewModel
    {
        private readonly IUacElevation _uacElevation;

        public CertificateExportViewModel(IUacElevation uacElevation, IInteractionService interactionService) : base(Resources.Localization.Dialogs_ExportCertificate_Title, interactionService)
        {
            this._uacElevation = uacElevation;

            this.InputPath = new ChangeableFileProperty(Resources.Localization.Dialogs_ExportCertificate_PackageOrCerFile, interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                Filter = new DialogFilterBuilder().WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.Msix).WithAll()
            };

            this.ExtractCertificate = new ChangeableFileProperty(Resources.Localization.Dialogs_ExportCertificate_Output_Cer_Path, interactionService, ChangeableFileProperty.ValidatePath)
            {
                Filter = new DialogFilterBuilder().WithCertificates(DialogFilterBuilderPackagesExtensions.CertificateTypes.Cer).WithAll(),
                OpenForSaving = true
            };
            
            this.OperationType = new ChangeableProperty<CertOperationType>();
            this.CanExtract = new ChangeableProperty<bool>();
            
            this.AddChildren(this.InputPath, this.ExtractCertificate, this.OperationType, this.CanExtract);

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.OperationType.ValueChanged += this.OperationTypeOnValueChanged;
            
            this.RegisterForCommandLineGeneration(this.OperationType, this.InputPath, this.ExtractCertificate);
        }

        protected override string GenerateSilentCommandLine()
        {
            switch (this.OperationType.CurrentValue)
            {
                case CertOperationType.Extract:
                {
                    var verb = new ExtractCertVerb
                    {
                        Output = this.ExtractCertificate.CurrentValue,
                        File = this.InputPath.CurrentValue
                    };

                    if (string.IsNullOrEmpty(verb.Output))
                    {
                        verb.Output = Resources.Localization.Dialogs_ExportCertificate_Cmd_OutputPath_Placeholder;
                    }

                    if (string.IsNullOrEmpty(verb.File))
                    {
                        verb.File = Resources.Localization.Dialogs_ExportCertificate_Cmd_InputPath_Placeholder;
                    }

                    return verb.ToCommandLineString();
                }
                
                case CertOperationType.Import:
                {
                    var verb = new TrustVerb
                    {
                        File = this.InputPath.CurrentValue
                    };
                        
                    if (string.IsNullOrEmpty(verb.File))
                    {
                        verb.File = Resources.Localization.Dialogs_ExportCertificate_Cmd_InputPath_Placeholder;
                    }

                    return verb.ToCommandLineString();
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        public ChangeableProperty<CertOperationType> OperationType { get; private set; }

        public ChangeableProperty<bool> CanExtract { get; private set; }
        
        private void OperationTypeOnValueChanged(object sender, EventArgs eventArgs)
        {
            this.OnPropertyChanged(nameof(IsAdminRequired));
            this.OnPropertyChanged(nameof(CanExtract));
            this.OnPropertyChanged(nameof(OkButtonLabel));
        }

        public AsyncProperty<CertificateViewModel> CertificateDetails { get; } = new AsyncProperty<CertificateViewModel>();

        public ChangeableFileProperty InputPath { get; }

        public ChangeableFileProperty ExtractCertificate { get; }

        public string OkButtonLabel
        {
            get
            {
                switch (this.OperationType.CurrentValue)
                {
                    case CertOperationType.Extract:
                        return Resources.Localization.Dialogs_ExportCertificate_OkLabel_Extract;
                    case CertOperationType.Import:
                        return Resources.Localization.Dialogs_ExportCertificate_OkLabel_Import;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public bool IsAdminRequired => this.OperationType.CurrentValue == CertOperationType.Import;

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            switch (this.OperationType.CurrentValue)
            {
                case CertOperationType.Import:
                {
                    await this._uacElevation.AsAdministrator<ISigningManager>().Trust(this.InputPath.CurrentValue, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case CertOperationType.Extract:
                {
                    await this._uacElevation.AsCurrentUser<ISigningManager>().ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.ExtractCertificate.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private async Task<CertificateViewModel> GetCertificateDetails(string msixFilePath, CancellationToken cancellationToken)
        {
            this.DisplayValidationErrors = true;
            
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this._uacElevation.AsHighestAvailable<ISigningManager>().GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);
            
            if (result == null)
            {
                return null;
            }

            return new CertificateViewModel(result);
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var value = (string)e.NewValue;
            if (string.IsNullOrEmpty(this.ExtractCertificate.CurrentValue))
            {
                this.ExtractCertificate.CurrentValue = value + ".cer";
            }
            
            var ext = Path.GetExtension(this.InputPath.CurrentValue);

            switch (ext?.ToLowerInvariant())
            {
                case FileConstants.MsixExtension:
                case FileConstants.AppxExtension:
                    this.CanExtract.CurrentValue = true;
                    break;
                default:
                    this.CanExtract.CurrentValue = false;
                    break;
            }

#pragma warning disable 4014
            this.CertificateDetails.Load(this.GetCertificateDetails(value, CancellationToken.None));
        }
    }
}

