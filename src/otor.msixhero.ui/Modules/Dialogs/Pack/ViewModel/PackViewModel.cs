using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.Pack.ViewModel
{
    public class PackViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxPacker appxPacker;
        private readonly IAppxSigningManager signingManager;
        private ICommand openSuccessLink;
        private ICommand reset;

        public PackViewModel(
            IAppxPacker appxPacker, 
            IAppxSigningManager signingManager,
            IConfigurationService configurationService,
            IInteractionService interactionService) : base("Pack MSIX package", interactionService)
        {
            this.appxPacker = appxPacker;
            this.signingManager = signingManager;
            var signConfig = configurationService.GetCurrentConfiguration().Signing ?? new SigningConfiguration();

            this.InputPath = new ChangeableFolderProperty(interactionService)
            {
                Validators = new[] { ChangeableFolderProperty.ValidatePath },
                ValidationMode = ValidationMode.Silent
            };

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                OpenForSaving = true,
                Filter = "MSIX/APPX packages|*.msix;*.appx|All files|*.*",
                ValidationMode = ValidationMode.Silent
            };

            this.Sign = new ChangeableProperty<bool>();
            this.Compress = new ChangeableProperty<bool>(true);
            this.Validate = new ChangeableProperty<bool>(true);

            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManager, signConfig, true);
            
            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.Sign.ValueChanged += this.SignOnValueChanged;

            this.AddChildren(this.InputPath, this.OutputPath, this.Sign, this.SelectedCertificate);
            this.SetValidationMode(ValidationMode.Silent, true);
        }
        
        public ChangeableFileProperty OutputPath { get; }

        public ChangeableFolderProperty InputPath { get; }
        
        public ICommand OpenSuccessLink
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand Reset
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public CertificateSelectorViewModel SelectedCertificate { get; }

        public ChangeableProperty<bool> Sign { get; }

        public ChangeableProperty<bool> Compress { get; }

        public ChangeableProperty<bool> Validate { get; }

        protected override async Task Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            AppxPackerOptions opts = 0;
            if (!this.Validate.CurrentValue)
            {
                opts |= AppxPackerOptions.NoValidation;
            }

            if (!this.Compress.CurrentValue)
            {
                opts |= AppxPackerOptions.NoCompress;
            }

            await this.appxPacker.Pack(this.InputPath.CurrentValue, this.OutputPath.CurrentValue, opts, cancellationToken, progress).ConfigureAwait(false);

            if (this.Sign.CurrentValue)
            {
                switch (this.SelectedCertificate.Store.CurrentValue)
                {
                    case CertificateSource.Personal:
                        await this.signingManager.SignPackage(this.OutputPath.CurrentValue, true, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue?.Model, this.SelectedCertificate.TimeStamp.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                    case CertificateSource.Pfx:
                        await this.signingManager.SignPackage(this.OutputPath.CurrentValue, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                }
            }
        }

        private void ResetExecuted(object parameter)
        {
            this.InputPath.Reset();
            this.OutputPath.Reset();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.OutputPath.CurrentValue);
        }

        private void SignOnValueChanged(object sender, ValueChangedEventArgs e)
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

