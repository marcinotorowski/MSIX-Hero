using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxSigningManager signingManager;
        private string publisherName;
        private string publisherFriendlyName;
        private string password;
        private string outputPath;
        
        public NewSelfSignedViewModel(IAppxSigningManager signingManager)
        {
            this.signingManager = signingManager;
        }

        public string PublisherName
        {
            get => this.publisherName;
            set => this.SetField(ref this.publisherName, value);
        }

        public string PublisherFriendlyName
        {
            get => this.publisherFriendlyName;
            set => this.SetField(ref this.publisherFriendlyName, value);
        }

        public string Password
        {
            get => this.password;
            set => this.SetField(ref this.password, value);
        }

        public string OutputPath
        {
            get => this.outputPath;
            set => this.SetField(ref this.outputPath, value);
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

        public Task Save()
        {
            return this.signingManager.CreateSelfSignedCertificate(new DirectoryInfo(this.OutputPath), this.PublisherName, this.PublisherFriendlyName, this.Password, CancellationToken.None);
        }

        public string Title
        {
            get => "New self signed certificate";
        }

        public event Action<IDialogResult> RequestClose;
    }
}
