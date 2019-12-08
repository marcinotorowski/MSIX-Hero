using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel
{
    public class CertificateExportViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxSigningManager signingManager;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool saveToFile = true;
        private bool saveToStore;
        private bool isSuccess;

        public CertificateExportViewModel(IAppxSigningManager signingManager, IInteractionService interactionService)
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

            this.ChangeableContainer = new ChangeableContainer(this.InputPath, this.OutputPath) { IsValidated = false };
        }

        public ChangeableContainer ChangeableContainer { get; }

        public AsyncProperty<CertificateViewModel> CertificateDetails { get; } = new AsyncProperty<CertificateViewModel>();

        public ChangeableFileProperty InputPath { get; }

        public ChangeableFileProperty OutputPath { get; }

        public bool SaveToFile
        {
            get => this.saveToFile;
            set
            {
                this.SetField(ref this.saveToFile, value);
                this.OutputPath.IsValidated = value;
            }
        }

        public bool SaveToStore
        {
            get => this.saveToStore;
            set
            {
                this.SetField(ref this.saveToStore, value);
            }
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public int Progress
        {
            get => this.progress;
            private set => this.SetField(ref this.progress, value);
        }

        public string ProgressMessage
        {
            get => this.progressMessage;
            private set => this.SetField(ref this.progressMessage, value);
        }

        public bool CanCloseDialog()
        {
            return true;
        }
        
        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
        public async Task Save()
        {
            this.InputPath.IsValidated = true;
            this.OutputPath.IsValidated = this.saveToFile;

            if (!this.ChangeableContainer.IsValid)
            {
                return;
            }

            var token = new Progress();

            EventHandler<ProgressData> handler = (sender, data) =>
            {
                this.Progress = data.Progress;
                this.ProgressMessage = data.Message;
            };

            this.IsLoading = true;
            try
            {
                token.ProgressChanged += handler;

                if (this.saveToFile)
                {
                    await this.signingManager.ExtractCertificateFromMsix(this.InputPath.CurrentValue, this.OutputPath.CurrentValue).ConfigureAwait(false);
                }

                if (this.saveToStore)
                {
                    await this.signingManager.ExtractCertificateFromMsix(this.InputPath.CurrentValue).ConfigureAwait(false);
                }

                this.IsSuccess = true;
            }
            finally
            {
                token.ProgressChanged -= handler;
                this.IsLoading = false;
                this.Progress = 100;
                this.ProgressMessage = null;
            }
        }

        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }

        public bool CanSave()
        {
            return this.ChangeableContainer.ValidationMessage == null;
        }

        public string Title
        {
            get => "Extract certificate";
        }

        public event Action<IDialogResult> RequestClose;

        private async Task<CertificateViewModel> GetCertificateDetails(string msixFilePath, CancellationToken cancellationToken)
        {
            var result = await this.signingManager.GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);
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

