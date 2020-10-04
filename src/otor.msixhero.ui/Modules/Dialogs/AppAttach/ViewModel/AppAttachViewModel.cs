using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Domain;

namespace Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.ViewModel
{
    public class AppAttachViewModel : ChangeableDialogViewModel
    {
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory;

        public AppAttachViewModel(ISelfElevationProxyProvider<IAppAttachManager> appAttachManagerFactory, IInteractionService interactionService) : base("Prepare VHD for app attach", interactionService)
        {
            this.appAttachManagerFactory = appAttachManagerFactory;
            this.interactionService = interactionService;
            this.InputPath = new ChangeableFileProperty("Source MSIX file", interactionService)
            {
                Filter = "MSIX files|*.msix"
            };

            this.GenerateScripts = new ChangeableProperty<bool>(true);

            this.InputPath.Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence };
            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.ExtractCertificate = new ChangeableProperty<bool>();
            this.SizeMode = new ChangeableProperty<AppAttachSizeMode>();
            this.FixedSize = new ValidatedChangeableProperty<string>("Fixed size", "100", this.ValidateFixedSize);
            this.AddChildren(this.InputPath, this.GenerateScripts, this.ExtractCertificate, this.FixedSize, this.SizeMode);
        }

        public ChangeableFileProperty InputPath { get; }

        public ChangeableProperty<bool> GenerateScripts { get; }

        public ChangeableProperty<bool> ExtractCertificate { get; }
        
        public ValidatedChangeableProperty<string> FixedSize { get; }

        public ChangeableProperty<AppAttachSizeMode> SizeMode { get; }

        public string OutputPath { get; private set; }
        
        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            if (!this.interactionService.SaveFile(this.OutputPath, "Virtual disks|*.vhd", out var output))
            {
                return false;
            }

            this.OutputPath = output;

            var sizeInMegabytes = this.SizeMode.CurrentValue == AppAttachSizeMode.Auto ? 0 : uint.Parse(this.FixedSize.CurrentValue);

            var appAttach = await this.appAttachManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await appAttach.CreateVolume(
                this.InputPath.CurrentValue,
                output,
                sizeInMegabytes,
                this.ExtractCertificate.CurrentValue,
                this.GenerateScripts.CurrentValue,
                cancellationToken,
                progress).ConfigureAwait(false);

            return true;
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                return;
            }

            this.OutputPath = Path.ChangeExtension(this.InputPath.CurrentValue, "vhd");
        }

        private string ValidateFixedSize(string value)
        {
            if (this.SizeMode.CurrentValue == AppAttachSizeMode.Auto)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                return "Fixed size cannot be empty.";
            }

            if (!int.TryParse(value, out var parsed))
            {
                return "Fixed size must ba a number";
            }

            return parsed <= 0 ? "Fixed size must be a positive number" : null;
        }
    }
}

