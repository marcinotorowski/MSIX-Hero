using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel
{
    public class CertificateExportViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxSigningManager signingManager;

        private ChangeableContainer customValidationContainer;

        public CertificateExportViewModel(IAppxSigningManager signingManager, IInteractionService interactionService) : base("Extract certificate", interactionService)
        {
            this.signingManager = signingManager;

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Filter = "MSIX files|*.msix"
            };

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.InputPath.Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence };

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                Filter = "Certificate files|*.cer",
                OpenForSaving = true,
                Validators = new [] { ChangeableFileProperty.ValidatePath }
            };

            this.SaveToFile = new ChangeableProperty<bool>(true);
            this.SaveToStore = new ChangeableProperty<bool>(true);

            customValidationContainer = new ChangeableContainer(this.SaveToFile, this.SaveToStore);
            customValidationContainer.CustomValidation += this.CustomCheckboxValidation;

            this.AddChildren(this.InputPath, this.OutputPath, customValidationContainer);
            this.SetValidationMode(ValidationMode.Silent, true);
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

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (this.SaveToFile.CurrentValue)
            {
                await this.signingManager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
            }

            if (this.SaveToStore.CurrentValue)
            {
                await this.signingManager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
            }

            return true;
        }

        private async Task<CertificateViewModel> GetCertificateDetails(string msixFilePath, CancellationToken cancellationToken)
        {
            var result = await this.signingManager.GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (this.customValidationContainer.ValidationMode == ValidationMode.Silent)
            {
                this.customValidationContainer.ValidationMode = ValidationMode.Silent;
            }
            else
            {
                this.customValidationContainer.ValidationMode = ValidationMode.Default;
            }

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

