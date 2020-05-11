using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.AppAttach.ViewModel
{
    public enum AppAttachSizeMode
    {
        Auto,
        Fixed
    }

    public class AppAttachViewModel : ChangeableDialogViewModel
    {
        private readonly IApplicationStateManager appState;
        private readonly IInteractionService interactionService;
        private readonly IProcessManager processManager;
        private readonly ISelfElevationManagerFactory<IAppAttachManager> appAttachManagerFactory;

        public AppAttachViewModel(IApplicationStateManager appState, ISelfElevationManagerFactory<IAppAttachManager> appAttachManagerFactory, IProcessManager processManager, IInteractionService interactionService) : base("Prepare VHD for app attach", interactionService)
        {
            this.appState = appState;
            this.appAttachManagerFactory = appAttachManagerFactory;
            this.processManager = processManager;
            this.interactionService = interactionService;
            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Filter = "MSIX files|*.msix"
            };

            this.GenerateScripts = new ChangeableProperty<bool>(true);

            this.InputPath.Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence };
            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.ExtractCertificate = new ChangeableProperty<bool>();
            this.SizeMode = new ChangeableProperty<AppAttachSizeMode>();
            this.FixedSize = new ValidatedChangeableProperty<string>("100")
            {
                Validators = new Func<string, string>[] { this.ValidateFixedSize }
            };
            
            this.AddChildren(this.InputPath, this.GenerateScripts, this.ExtractCertificate, this.FixedSize, this.SizeMode);
            this.SetValidationMode(ValidationMode.Silent, true);
        }

        public ChangeableFileProperty InputPath { get; }

        public ChangeableProperty<bool> GenerateScripts { get; }

        public ChangeableProperty<bool> ExtractCertificate { get; }
        
        public ValidatedChangeableProperty<string> FixedSize { get; }

        public ChangeableProperty<AppAttachSizeMode> SizeMode { get; }

        public string OutputPath { get; private set; }
        
        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.interactionService.SaveFile(this.OutputPath, "Virtual disks|*.vhd", out var output))
            {
                return false;
            }

            this.SetValidationMode(ValidationMode.Default, true);
            if (!this.IsValid)
            {
                return false;
            }

            this.OutputPath = output;

            var sizeInMegabytes = this.SizeMode.CurrentValue == AppAttachSizeMode.Auto ? 0 : uint.Parse(this.FixedSize.CurrentValue);

            if (this.appState.CurrentState.IsElevated)
            {
                var appAttach = await this.appAttachManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                await appAttach.CreateVolume(
                    this.InputPath.CurrentValue, 
                    output, 
                    sizeInMegabytes, 
                    this.ExtractCertificate.CurrentValue,
                    this.GenerateScripts.CurrentValue, 
                    cancellationToken, 
                    progress).ConfigureAwait(false);
            }
            else
            {
                var cmd = new ConvertToVhd(this.InputPath.CurrentValue, output)
                {
                    GenerateScripts = this.GenerateScripts.CurrentValue,
                    ExtractCertificate = this.ExtractCertificate.CurrentValue,
                    SizeInMegaBytes = sizeInMegabytes
                };

                var mgr = await this.appAttachManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                await mgr.CreateVolume(this.InputPath.CurrentValue, output, sizeInMegabytes, this.ExtractCertificate.CurrentValue, this.GenerateScripts.CurrentValue, cancellationToken).ConfigureAwait(false);
            }

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

