using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel
{
    public class CertificateExportViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private string inputPath;
        private string outputPath;
        private bool saveToFile = true;
        private bool saveToStore;
        private bool isSuccess;
        private ICommand browseForInput;
        private ICommand browseForOutput;

        public CertificateExportViewModel(IAppxSigningManager signingManager, IInteractionService interactionService)
        {
            this.signingManager = signingManager;
            this.interactionService = interactionService;
        }

        public AsyncProperty<CertificateViewModel> CertificateDetails { get; } = new AsyncProperty<CertificateViewModel>();

        public string InputPath
        {
            get => this.inputPath;
            set
            {
                this.SetField(ref this.inputPath, value);

                if (string.IsNullOrEmpty(this.outputPath))
                {
                    this.OutputPath = value + ".cer";
                }

                this.OnPropertyChanged(nameof(Error));
                
#pragma warning disable 4014
                this.CertificateDetails.Load(this.GetCertificateDetails(value, CancellationToken.None));
#pragma warning restore 4014
            }
        }

        public string OutputPath
        {
            get => this.outputPath;
            set
            {
                this.SetField(ref this.outputPath, value);
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public bool SaveToFile
        {
            get => this.saveToFile;
            set
            {
                this.SetField(ref this.saveToFile, value);
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public bool SaveToStore
        {
            get => this.saveToStore;
            set
            {
                this.SetField(ref this.saveToStore, value);
                this.OnPropertyChanged(nameof(Error));
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
                    await this.signingManager.ExtractCertificateFromMsix(this.inputPath, this.outputPath).ConfigureAwait(false);
                }

                if (this.saveToStore)
                {
                    await this.signingManager.ExtractCertificateFromMsix(this.inputPath).ConfigureAwait(false);
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

        public string Error => this[nameof(this.InputPath)] ?? this[nameof(this.OutputPath)];

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputPath):
                        if (string.IsNullOrEmpty(this.InputPath))
                        {
                            return "The path to the MSIX package may not be empty.";
                        }

                        break;

                    case nameof(this.OutputPath):
                        if (this.SaveToFile && string.IsNullOrEmpty(this.OutputPath))
                        {
                            return "The path to the output .CER file may not be empty.";
                        }

                        break;
                }

                return null;
            }
        }

        public bool CanSave()
        {
            return this.Error == null && (this.saveToFile || this.saveToStore);
        }

        public string Title
        {
            get => "Extract certificate";
        }

        public ICommand BrowseForInput
        {
            get => this.browseForInput ?? (this.browseForInput = new DelegateCommand(param => this.BrowseForInputExecute(), param => true));
        }

        public ICommand BrowseForOutput
        {
            get => this.browseForOutput ?? (this.browseForOutput = new DelegateCommand(param => this.BrowseForOutputExecute(), param => true));
        }

        private void BrowseForInputExecute()
        {
            if (!this.interactionService.SelectFile(filterString: "MSIX files (*.msix)|*.msix", out var selectedFile))
            {
                return;
            }

            this.InputPath = selectedFile;
        }

        private void BrowseForOutputExecute()
        {
            if (!this.interactionService.SaveFile(filterString: "CER files (*.cer)|*.cer", out var selectedFile))
            {
                return;
            }

            this.OutputPath = selectedFile;
        }

        public event Action<IDialogResult> RequestClose;

        private async Task<CertificateViewModel> GetCertificateDetails(string msixFilePath, CancellationToken cancellationToken)
        {
            var result = await this.signingManager.GetCertificateFromMsix(msixFilePath, cancellationToken).ConfigureAwait(false);
            return new CertificateViewModel(result);
        }
    }
}

