using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private string publisherName;
        private string publisherFriendlyName;
        private string password;
        private string outputPath;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSubjectTouched;
        private bool isSuccess;
        private ICommand importNewCertificate;

        public NewSelfSignedViewModel(
            IAppxSigningManager signingManager, 
            IInteractionService interactionService, 
            IApplicationStateManager stateManager,
            IConfigurationService configurationService)
        {
            this.signingManager = signingManager;
            this.interactionService = interactionService;
            this.stateManager = stateManager;
            this.publisherName = "CN=";
            this.outputPath = configurationService.GetCurrentConfiguration().Signing?.DefaultOutFolder;
        }

        public string PublisherName
        {
            get => this.publisherName;
            set
            {
                this.SetField(ref this.publisherName, value);
                this.OnPropertyChanged(nameof(Error));
                this.isSubjectTouched = true;
            }
        }

        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }

        public string PublisherFriendlyName
        {
            get => this.publisherFriendlyName;
            set
            {
                this.SetField(ref this.publisherFriendlyName, value);
                this.OnPropertyChanged(nameof(Error));
                if (this.isSubjectTouched)
                {
                    return;
                }

                this.SetField(ref this.publisherName, "CN=" + this.PublisherName);
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

        public string Password
        {
            get => this.password;
            set
            {
                this.SetField(ref this.password, value);
                this.OnPropertyChanged(nameof(Error));
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

        public ICommand ImportNewCertificate
        {
            get => this.importNewCertificate ??= new DelegateCommand(param => this.ImportNewCertificateExecute());
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
                await this.signingManager.CreateSelfSignedCertificate(new DirectoryInfo(this.OutputPath), this.PublisherName, this.PublisherFriendlyName, this.Password, CancellationToken.None).ConfigureAwait(true);

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

        public string Error =>
            this[nameof(PublisherName)] ??
            this[nameof(PublisherFriendlyName)] ??
            this[nameof(OutputPath)] ??
            this[nameof(Password)];

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(PublisherName):
                        return string.IsNullOrEmpty(this.PublisherName) ? "The display name of the publisher may not be empty." : null;
                    case nameof(PublisherFriendlyName):
                        if (string.IsNullOrEmpty(this.PublisherFriendlyName))
                        {
                            return "The name of the publisher may not be empty.";
                        }

                        if (!this.PublisherName.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                        {
                            return "Publisher name must start with CN=";
                        }

                        return null;
                    case nameof(OutputPath):
                        return string.IsNullOrEmpty(this.OutputPath) ? "The output path may not be empty." : null;
                    case nameof(Password):
                        return string.IsNullOrEmpty(this.Password) ? "The password may not be empty." : null;
                }

                return null;
            }
        }

        public bool CanSave()
        {
            return this.Error == null;
        }

        public string Title
        {
            get => "New self signed certificate";
        }

        private void ImportNewCertificateExecute()
        {
            var file = Directory.EnumerateFiles(this.OutputPath, "*.cer").OrderByDescending(d => new FileInfo(d).LastWriteTimeUtc).FirstOrDefault();
            if (file == null)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new InstallCertificate(file));
        }

        public event Action<IDialogResult> RequestClose;
    }
}

