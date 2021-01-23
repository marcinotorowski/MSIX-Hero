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
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
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
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;

        public CertificateExportViewModel(ISelfElevationProxyProvider<ISigningManager> signingManagerFactory, IInteractionService interactionService) : base("Extract certificate", interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;

            this.InputPath = new ChangeableFileProperty("Path to signed MSIX file", interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                Filter = "All supported files|*.msix;*.cer|MSIX files|*.msix|Certificates|*.cer"
            };

            this.ExtractCertificate = new ChangeableFileProperty("Path to certificate", interactionService, ChangeableFileProperty.ValidatePath)
            {
                Filter = "Certificate files|*.cer",
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
                        verb.Output = "<output-path>";
                    }

                    if (string.IsNullOrEmpty(verb.File))
                    {
                        verb.File = "<input-path>";
                    }

                    return verb.ToCommandLineString();
                }
                
                case CertOperationType.Import:
                {
                    var verb = new TrustVerb()
                    {
                        File = this.InputPath.CurrentValue
                    };
                        
                    if (string.IsNullOrEmpty(verb.File))
                    {
                        verb.File = "<input-path>";
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
                        return "Extract certificate";
                    case CertOperationType.Import:
                        return "Import certificate";
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
                    var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
                    await manager.Trust(this.InputPath.CurrentValue, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case CertOperationType.Extract:
                {
                    var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                    await manager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.ExtractCertificate.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
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

            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var result = await manager.GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);

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
            if (!string.Equals(".msix", ext, StringComparison.OrdinalIgnoreCase))
            {
                this.CanExtract.CurrentValue = false;
            }
            else
            {
                this.CanExtract.CurrentValue = true;
            }

#pragma warning disable 4014
            this.CertificateDetails.Load(this.GetCertificateDetails(value, CancellationToken.None));
        }
    }
}

