using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private string pfxPath;
        private string timestamp = "http://timestamp.globalsign.com/scripts/timstamp.dll";
        private CertificateViewModel selectedPersonalCertificate;
        private CertificateSource store;
        private SecureString password;

        public PackageSigningViewModel(IAppxSigningManager signingManager, IInteractionService interactionService)
        {
            this.signingManager = signingManager;
            this.interactionService = interactionService;
            this.Files = new ObservableCollection<string>();
            this.Files.CollectionChanged += FilesOnCollectionChanged;
            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates());
        }

        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public CertificateViewModel SelectedPersonalCertificate
        {
            get => selectedPersonalCertificate;
            set
            {
                this.SetField(ref this.selectedPersonalCertificate, value);
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

        public SecureString Password
        {
            get => this.password;
            set
            {
                if (this.password != null)
                {
                    this.password.Dispose();
                }

                this.SetField(ref this.password, value);
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public CertificateSource Store
        {
            get => this.store;
            set
            {
                this.SetField(ref this.store, value);
                this.OnPropertyChanged(nameof(Error));
                this.OnPropertyChanged(nameof(PfxPath));
                this.OnPropertyChanged(nameof(SelectedPersonalCertificate));
            }
        }

        private async Task<ObservableCollection<CertificateViewModel>> LoadPersonalCertificates(CancellationToken cancellationToken = default)
        {
            var certs = await this.signingManager.GetCertificatesFromStore(CertificateStoreType.Both, cancellationToken).ConfigureAwait(false);
            var result = new ObservableCollection<CertificateViewModel>(certs.Select(c => new CertificateViewModel(c)));
            this.selectedPersonalCertificate = result.FirstOrDefault();
            return result;
        }

        public void OnDialogClosed()
        {
        }

        public void BrowseForFile()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Path", out var file))
            {
                this.Files.Add(file);
            }
            else
            {
                var interactionResult = this.interactionService.SelectFiles(filterString: "MSIX packages (*.msix)|*.msix", out string[] selection);
                if (!interactionResult || !selection.Any())
                {
                    return;
                }

                foreach (var selected in selection)
                {
                    if (this.Files.Contains(selected))
                    {
                        continue;
                    }

                    this.Files.Add(selected);
                }
            }
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
                // TODO

                foreach (var file in this.Files)
                {
                    if (this.Store == CertificateSource.Pfx)
                    {
                        await this.signingManager.SignPackage(file, true, this.pfxPath, this.Password, this.TimeStamp, CancellationToken.None, token);
                    }
                    else
                    {
                        await this.signingManager.SignPackage(file, true, this.SelectedPersonalCertificate.Model, this.TimeStamp, CancellationToken.None, token);
                    }
                }
            }
            finally
            {
                token.ProgressChanged -= handler;
                this.IsLoading = false;
                this.Progress = 100;
                this.ProgressMessage = null;
            }
        }

        public ObservableCollection<string> Files { get; }

        public string Error => this[nameof(SelectedPersonalCertificate)] ?? this[nameof(PfxPath)] ?? this[nameof(Files)] ?? this[nameof(TimeStamp)];

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.PfxPath):

                        if (this.Store == CertificateSource.Pfx && string.IsNullOrEmpty(this.PfxPath))
                        {
                            return "File path to PFX is required.";
                        }
                    
                        break;

                    case nameof(this.SelectedPersonalCertificate):

                        if (this.Store == CertificateSource.Personal && this.SelectedPersonalCertificate == null)
                        {
                            return "Certificate selection is required.";
                        }

                        break;

                    case nameof(this.TimeStamp):
                        if (!string.IsNullOrEmpty(this.timestamp))
                        {
                            if (!Uri.TryCreate(this.timestamp, UriKind.Absolute, out _))
                            {
                                return "This must be a valid URL, or an empty value for no timestamp.";
                            }
                        }
                        break;

                    case nameof(this.Files):

                        if (!this.Files.Any())
                        {
                            return "At least one file is required.";
                        }

                        break;
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
            get => "Package Signing";
        }

        public string PfxPath
        {
            get => this.pfxPath;
            set
            {
                this.SetField(ref this.pfxPath, value);
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public string TimeStamp
        {
            get => this.timestamp;
            set
            {
                this.SetField(ref this.timestamp, value);
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public event Action<IDialogResult> RequestClose;

        private void FilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(Error));
        }
    }
}

