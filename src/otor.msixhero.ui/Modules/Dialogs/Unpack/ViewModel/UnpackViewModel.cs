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
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.Unpack.ViewModel
{
    public class UnpackViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxPacker appxPacker;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool createFolder;
        private bool isSuccess;
        private ICommand openSuccessLink;
        private ICommand reset;

        public UnpackViewModel(IAppxPacker appxPacker, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.appxPacker = appxPacker;

            var initialOut = configurationService.GetCurrentConfiguration().Packer?.DefaultOutFolder;
            this.OutputPath = new ChangeableFolderProperty(interactionService)
            {
                Validator = ChangeableFolderProperty.ValidatePath,
                CurrentValue = initialOut,
                IsValidated = true
            };

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Validator = ChangeableFileProperty.ValidatePath,
                Filter = "MSIX/APPX packages|*.msix;*.appx|All files|*.*",
                IsValidated = true
            };

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.ChangeableContainer = new ChangeableContainer(this.InputPath, this.OutputPath)
            {
                IsValidated = true
            };
        }

        public ChangeableContainer ChangeableContainer { get; }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                return;
            }

            var newFilePath = new FileInfo((string) e.NewValue);

            // ReSharper disable once PossibleNullReferenceException
            var directory = newFilePath.Directory.FullName;

            var fileName = newFilePath.Name;

            this.OutputPath.CurrentValue = Path.Join(directory, fileName + "_extracted");
        }

        public ChangeableFolderProperty OutputPath { get; }

        public ChangeableFileProperty InputPath { get; }
        
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

        public bool CreateFolder
        {
            get => this.createFolder;
            set => this.SetField(ref this.createFolder, value);
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
            get => "Unpack MSIX package";
        }

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

                await this.appxPacker.Unpack(this.InputPath.CurrentValue, this.GetOutputPath(), default, token).ConfigureAwait(false);
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
            Process.Start("explorer.exe", "/select," + this.GetOutputPath());
        }

        private string GetOutputPath()
        {
            if (!string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                return this.CreateFolder ? Path.Combine(this.OutputPath.CurrentValue, Path.GetFileNameWithoutExtension(this.InputPath.CurrentValue)) : this.OutputPath.CurrentValue;
            }

            return null;
        }
    }
}

