using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Managers;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IAppxSigningManager signingManager;
        private string publisherName;
        private string publisherFriendlyName;
        private string password;
        private string outputPath;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSubjectTouched;

        public NewSelfSignedViewModel(IAppxSigningManager signingManager)
        {
            this.signingManager = signingManager;
            this.publisherName = "CN=";
            this.outputPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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

        public event Action<IDialogResult> RequestClose;
    }
}

