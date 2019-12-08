using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IApplicationStateManager stateManager;
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
            this.stateManager = stateManager;
            
            this.OutputPath = new ChangeableFolderProperty(interactionService, configurationService.GetCurrentConfiguration().Signing?.DefaultOutFolder);
            
            this.PublisherName = new ValidatedChangeableProperty<string>("CN=");
            this.PublisherName.ValueChanged += this.PublisherNameOnValueChanged;
            this.PublisherName.Validators = new Func<string, string>[] { ValidatePublisherName };
            
            this.PublisherFriendlyName = new ValidatedChangeableProperty<string>();
            this.PublisherFriendlyName.ValueChanged += this.PublisherFriendlyNameOnValueChanged;
            this.PublisherFriendlyName.Validators = new Func<string, string>[] { ValidatePublisherFriendlyName };

            this.Password = new ValidatedChangeableProperty<string>();
            this.Password.Validators = new Func<string, string>[] { ValidatePassword };
            
            this.ChangeableContainer = new ChangeableContainer(this.OutputPath, this.Password, this.PublisherName, this.PublisherFriendlyName)
            {
                IsValidated = false
            };
        }

        public ChangeableContainer ChangeableContainer { get; }

        public ValidatedChangeableProperty<string> PublisherName { get; }
        
        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }

        public ValidatedChangeableProperty<string> PublisherFriendlyName { get; }

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

        public ValidatedChangeableProperty<string> Password { get; }

        public ChangeableFolderProperty OutputPath { get; }

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
                await this.signingManager.CreateSelfSignedCertificate(
                    new DirectoryInfo(this.OutputPath.CurrentValue), 
                    this.PublisherName.CurrentValue, 
                    this.PublisherFriendlyName.CurrentValue, 
                    this.Password.CurrentValue, 
                    CancellationToken.None).ConfigureAwait(true);

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

        public bool CanSave()
        {
            return this.ChangeableContainer.IsValid;
        }

        public string Title
        {
            get => "New self signed certificate";
        }

        public event Action<IDialogResult> RequestClose;

        private static string ValidatePublisherName(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The display name of the publisher may not be empty.";
            }
            
            if (!newValue.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                return "Publisher name must start with CN=";
            }

            return null;
        }

        private static string ValidatePublisherFriendlyName(string newValue)
        {
            return string.IsNullOrEmpty(newValue) ? "The name of the publisher may not be empty." : null;
        }

        private static string ValidatePassword(string currentValue)
        {
            return string.IsNullOrEmpty(currentValue) ? "The password may not be empty." : null;
        }

        private void ImportNewCertificateExecute()
        {
            var file = Directory.EnumerateFiles(this.OutputPath.CurrentValue, "*.cer").OrderByDescending(d => new FileInfo(d).LastWriteTimeUtc).FirstOrDefault();
            if (file == null)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new InstallCertificate(file));
        }
        
        private void PublisherNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.isSubjectTouched = true;
        }

        private void PublisherFriendlyNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.isSubjectTouched)
            {
                return;
            }

            var touch = this.isSubjectTouched;
            try
            {
                this.PublisherName.CurrentValue = "CN=" + (string) e.NewValue;
            }
            finally
            {
                this.isSubjectTouched = touch;
            }
        }
    }
}

