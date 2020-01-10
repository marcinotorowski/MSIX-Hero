using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.Pack.ViewModel
{
    public class PackViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxPacker appxPacker;
        private readonly IAppxSigningManager signingManager;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSuccess;
        private ICommand openSuccessLink;
        private ICommand reset;

        public PackViewModel(
            IAppxPacker appxPacker, 
            IAppxSigningManager signingManager,
            IConfigurationService configurationService,
            IInteractionService interactionService)
        {
            this.appxPacker = appxPacker;
            this.signingManager = signingManager;
            var signConfig = configurationService.GetCurrentConfiguration().Signing ?? new SigningConfiguration();

            this.InputPath = new ChangeableFolderProperty(interactionService)
            {
                Validators = new[] { ChangeableFolderProperty.ValidatePath },
                IsValidated = true
            };

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                OpenForSaving = true,
                Filter = "MSIX/APPX packages|*.msix;*.appx|All files|*.*",
                IsValidated = true
            };

            this.Sign = new ChangeableProperty<bool>(false);
            this.Compress = new ChangeableProperty<bool>(true);
            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManager, signConfig, true);
            
            this.ChangeableContainer = new ChangeableContainer(this.InputPath, this.OutputPath, this.Sign, this.SelectedCertificate)
            {
                IsValidated = false
            };

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.Sign.ValueChanged += this.SignOnValueChanged;
        }

        public ChangeableContainer ChangeableContainer { get; }

        public ChangeableFileProperty OutputPath { get; }

        public ChangeableFolderProperty InputPath { get; }
        
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

        public ICommand OpenSuccessLink
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand Reset
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }
        
        public bool CanSave()
        {
            return this.ChangeableContainer.IsValid;
        }

        public string Title
        {
            get => "Pack MSIX package";
        }

        public CertificateSelectorViewModel SelectedCertificate { get; }

        public ChangeableProperty<bool> Sign { get; }

        public ChangeableProperty<bool> Compress { get; }

        public event Action<IDialogResult> RequestClose;

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
            this.ChangeableContainer.IsValidated = true;
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

                await this.appxPacker.Pack(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, this.Compress.CurrentValue).ConfigureAwait(false);

                if (this.Sign.CurrentValue)
                {
                    switch (this.SelectedCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Personal:
                            await this.signingManager.SignPackage(this.OutputPath.CurrentValue, true, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue?.Model, this.SelectedCertificate.TimeStamp.CurrentValue).ConfigureAwait(false);
                            break;
                        case CertificateSource.Pfx:
                            await this.signingManager.SignPackage(this.OutputPath.CurrentValue, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue).ConfigureAwait(false);
                            break;
                    }
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

        private void ResetExecuted(object parameter)
        {
            this.InputPath.Reset();
            this.OutputPath.Reset();
            this.IsSuccess = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.OutputPath.CurrentValue);
        }

        private void SignOnValueChanged(object? sender, ValueChangedEventArgs e)
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
            this.OutputPath.CurrentValue = Path.Join(newValue, "_packed", Path.GetFileName(newValue.TrimEnd('\\'))) + ".msix";
        }
    }
}

