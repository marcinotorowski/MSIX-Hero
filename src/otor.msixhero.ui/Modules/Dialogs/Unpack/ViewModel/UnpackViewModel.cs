using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.Unpack.ViewModel
{
    public class UnpackViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxPacker appxPacker;
        private ICommand openSuccessLink;
        private ICommand reset;

        public UnpackViewModel(IAppxPacker appxPacker, IInteractionService interactionService, IConfigurationService configurationService) : base("Unpack MSIX package", interactionService)
        {
            this.appxPacker = appxPacker;

            var initialOut = configurationService.GetCurrentConfiguration().Packer?.DefaultOutFolder;
            this.OutputPath = new ChangeableFolderProperty(interactionService, initialOut)
            {
                Validators = new[] { ChangeableFolderProperty.ValidatePath }
            };

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                Filter = "MSIX/APPX packages|*.msix;*.appx|All files|*.*"
            };
            
            this.CreateFolder = new ChangeableProperty<bool>(true);

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.AddChildren(this.InputPath, this.OutputPath, this.CreateFolder);
            this.SetValidationMode(ValidationMode.Silent, true);
        }
        
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
        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ChangeableProperty<bool> CreateFolder { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            await this.appxPacker.Unpack(this.InputPath.CurrentValue, this.GetOutputPath(), default, progress).ConfigureAwait(false);
            return true;
        }

        private void ResetExecuted(object parameter)
        {
            this.InputPath.Reset();
            this.OutputPath.Reset();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        { 
            Process.Start("explorer.exe", this.GetOutputPath());
        }

        private string GetOutputPath()
        {
            if (!string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                return this.CreateFolder.CurrentValue ? Path.Combine(this.OutputPath.CurrentValue, Path.GetFileNameWithoutExtension(this.InputPath.CurrentValue)) : this.OutputPath.CurrentValue;
            }

            return null;
        }
    }
}

