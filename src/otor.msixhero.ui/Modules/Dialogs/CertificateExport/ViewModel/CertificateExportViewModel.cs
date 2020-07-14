using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel
{
    public class CertificateExportViewModel : ChangeableDialogViewModel
    {
        private readonly IApplicationStateManager stateManager;
        private readonly ISelfElevationManagerFactory<ISigningManager> signingManagerFactory;

        private readonly ChangeableContainer customValidationContainer;

        public CertificateExportViewModel(IApplicationStateManager stateManager, ISelfElevationManagerFactory<ISigningManager> signingManagerFactory, IInteractionService interactionService) : base("Extract certificate", interactionService)
        {
            this.stateManager = stateManager;
            this.signingManagerFactory = signingManagerFactory;

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
            this.SaveToStore.ValueChanged += this.SaveToStoreOnValueChanged;

            customValidationContainer = new ChangeableContainer(this.SaveToFile, this.SaveToStore);
            customValidationContainer.CustomValidation += this.CustomCheckboxValidation;

            this.AddChildren(this.InputPath, this.OutputPath, customValidationContainer);
            this.SetValidationMode(ValidationMode.Silent, true);
        }

        private void SaveToStoreOnValueChanged(object? sender, ValueChangedEventArgs e)
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
                var manager = await this.signingManagerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                await manager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (this.SaveToStore.CurrentValue)
            {
                var command = new TrustPublisher(this.InputPath.CurrentValue);
                await this.stateManager.CommandExecutor.ExecuteAsync(command, cancellationToken, progress).ConfigureAwait(false);
            }

            return true;
        }

        private async Task<CertificateViewModel> GetCertificateDetails(string msixFilePath, CancellationToken cancellationToken)
        {
            var manager = await this.signingManagerFactory.Get(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var result = await manager.GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);

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

