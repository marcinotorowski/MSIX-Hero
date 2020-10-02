using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Helpers;
using Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.CertificateExport.ViewModel
{
    public class CertificateExportViewModel : ChangeableDialogViewModel
    {
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;

        private readonly ChangeableContainer customValidationContainer;

        public CertificateExportViewModel(ISelfElevationProxyProvider<ISigningManager> signingManagerFactory, IInteractionService interactionService) : base("Extract certificate", interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                DisplayName = "Path to signed MSIX file",
                Filter = "MSIX files|*.msix",
                Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence }
            };

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                DisplayName = "Path to certificate",
                Filter = "Certificate files|*.cer",
                OpenForSaving = true,
                Validators = new[] { ChangeableFileProperty.ValidatePath }
            };
            
            this.SaveToFile = new ChangeableProperty<bool>(true);
            this.SaveToStore = new ChangeableProperty<bool>(true);

            customValidationContainer = new ChangeableContainer(this.SaveToFile, this.SaveToStore);

            this.AddChildren(this.InputPath, this.OutputPath, customValidationContainer);

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.SaveToStore.ValueChanged += this.SaveToStoreOnValueChanged;
            customValidationContainer.CustomValidation += this.CustomCheckboxValidation;
        }

        private void SaveToStoreOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsAdminRequired));
        }

        private void CustomCheckboxValidation(object sender, ContainerValidationArgs e)
        {
            if (!e.IsValid)
            {
                return;
            }

            if (!this.SaveToFile.CurrentValue && !this.SaveToStore.CurrentValue)
            {
                e.SetError("Please select where to save the certificate (file or cert store)");
            }
            else if (this.CertificateDetails.HasValue && this.CertificateDetails.CurrentValue == null)
            {
                e.SetError("The selected file is unsigned.");
            }
        }

        public AsyncProperty<CertificateViewModel> CertificateDetails { get; } = new AsyncProperty<CertificateViewModel>();

        public ChangeableFileProperty InputPath { get; }

        public ChangeableFileProperty OutputPath { get; }

        public ChangeableProperty<bool> SaveToFile { get; }

        public ChangeableProperty<bool> SaveToStore { get; }

        public bool IsAdminRequired => this.SaveToStore.CurrentValue;

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (this.SaveToFile.CurrentValue)
            {
                var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                await manager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (this.SaveToStore.CurrentValue)
            {
                var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                await manager.Trust(this.InputPath.CurrentValue, cancellationToken).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                this.OutputPath.CurrentValue = value + ".cer";
            }

#pragma warning disable 4014
            this.CertificateDetails.Load(this.GetCertificateDetails(value, CancellationToken.None));
        }
    }
}

