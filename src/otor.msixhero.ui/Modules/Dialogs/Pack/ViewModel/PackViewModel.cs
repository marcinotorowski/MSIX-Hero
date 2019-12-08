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

namespace otor.msixhero.ui.Modules.Dialogs.Pack.ViewModel
{
    public class PackViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxPacker appxPacker;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSuccess;
        private ICommand openSuccessLink;
        private ICommand reset;

        public PackViewModel(IAppxPacker appxPacker, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.appxPacker = appxPacker;
            this.interactionService = interactionService;

            var initialOut = configurationService.GetCurrentConfiguration().Packer?.DefaultOutFolder;
            this.InputPath = new ChangeableFolderProperty(interactionService)
            {
                Validator = ChangeableFolderProperty.ValidatePath,
                CurrentValue = initialOut,
                IsValidated = true
            };

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                Validator = ChangeableFileProperty.ValidatePath,
                OpenForSaving = true,
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

                await this.appxPacker.Pack(this.InputPath.CurrentValue, this.OutputPath.CurrentValue).ConfigureAwait(false);
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
            Process.Start("explorer.exe", "/select," + this.OutputPath);
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                return;
            }

            var newFilePath = new DirectoryInfo((string)e.NewValue);
            this.OutputPath.CurrentValue = Path.Join(newFilePath.FullName.TrimEnd('\\') + "_packed");
        }
    }
}

