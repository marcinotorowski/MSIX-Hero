using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.Progress;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Winget.Yaml;
using Otor.MsixHero.Winget.Yaml.Entities;
using Prism.Commands;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Winget.ViewModel
{
    public class WingetInstallerViewModel : ChangeableContainer
    {
        private readonly IInteractionService interactionService;
        protected YamlUtils YamlUtils = new YamlUtils();

        private ICommand generateSha256, openSha256;
        private bool isGenerateHashShown;

        public WingetInstallerViewModel(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.AddChildren(
                this.Architecture = new ChangeableProperty<YamlArchitecture>(),
                this.Url = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateUrl(true, "Installer URL")),
                this.Sha256 = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateSha256(true, "Installer hash")),
                this.SystemAppId = new ValidatedChangeableProperty<string>((string)null, this.ValidateSystemAppId),
                this.SignatureSha256 = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateSha256(false, "Signature hash")),
                this.Scope = new ChangeableProperty<YamlScope?>(),
                this.SilentCommand = new ChangeableProperty<string>(),
                this.CustomCommand = new ChangeableProperty<string>(),
                this.InstallerType = new ValidatedChangeableProperty<YamlInstallerType>(ValidateInstallerType),
                this.SilentCommandWithProgress = new ChangeableProperty<string>()
            );

            this.InstallerType.ValueChanged += InstallerTypeOnValueChanged;

            this.SetValidationMode(ValidationMode.Silent, true);
        }

        public void SetData(YamlInstaller installer, bool useNullValues = true)
        {
            this.Model = installer;

            if (useNullValues || installer.Arch != null)
            {
                this.Architecture.CurrentValue = installer?.Arch ?? YamlArchitecture.none;
            }

            if (useNullValues || installer.Url != null)
            {
                this.Url.CurrentValue = installer?.Url;
            }

            if (useNullValues || installer.Sha256 != null)
            {
                this.Sha256.CurrentValue = installer?.Sha256;
            }

            if (useNullValues || installer.SystemAppId != null)
            {
                this.SystemAppId.CurrentValue = installer?.SystemAppId;
            }

            if (useNullValues || installer.SignatureSha256 != null)
            {
                this.SignatureSha256.CurrentValue = installer?.SignatureSha256;
            }

            if (useNullValues || installer.Scope != null)
            {
                this.Scope.CurrentValue = installer?.Scope ?? YamlScope.none;
            }

            if (useNullValues || installer.Switches?.Silent != null)
            {
                this.SilentCommand.CurrentValue = installer?.Switches?.Silent;
            }

            if (useNullValues || installer.Switches?.Custom != null)
            {
                this.CustomCommand.CurrentValue = installer?.Switches?.Custom;
            }

            if (useNullValues || installer.InstallerType != null)
            {
                this.InstallerType.CurrentValue = installer?.InstallerType ?? YamlInstallerType.none;
            }

            if (useNullValues || installer.Arch != null)
            {
                this.SilentCommandWithProgress.CurrentValue = installer?.Switches?.SilentWithProgress;
            }

            this.Commit();
            this.SetValidationMode(ValidationMode.Silent, true);
        }

        public ChangeableProperty<YamlArchitecture> Architecture { get; }

        public ChangeableProperty<YamlInstallerType> InstallerType { get; }

        public ChangeableProperty<string> Url { get; }
        
        public ChangeableProperty<YamlScope?> Scope { get; }
        
        public ChangeableProperty<string> Sha256 { get; }

        public ChangeableProperty<string> SystemAppId { get; }

        public ValidatedChangeableProperty<string> SignatureSha256 { get; }

        public ChangeableProperty<string> SilentCommand { get; }

        public ChangeableProperty<string> CustomCommand { get; }

        public ChangeableProperty<string> SilentCommandWithProgress { get; }

        public bool IsGenerateHashShown
        {
            get => this.isGenerateHashShown;
            set => this.SetField(ref this.isGenerateHashShown, value);
        }

        public YamlInstaller Model { get; set; }

        public ICommand GenerateSha256
        {
            get
            {
                return this.generateSha256 ??= new DelegateCommand<string>(this.GenerateHash);
            }
        }

        public ICommand OpenSha256
        {
            get
            {
                return this.openSha256 ??= new DelegateCommand<string>(this.OpenHash);
            }
        }

        private async void GenerateHash(string parameter)
        {
            if (string.IsNullOrEmpty(this.Url.CurrentValue))
            {
                this.interactionService.ShowError("You must first configure the installer URL before a hash can be calculated.");
                return;
            }
            
            if (this.interactionService.Confirm($"This will download the file '{this.Url.CurrentValue}' and calculate its hash. The download may take a while, do you want to continue?",  type: InteractionType.Question, buttons: InteractionButton.YesNo) == InteractionResult.No)
            {
                return;
            }

            var progress = new Progress();
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    if (parameter == "installer")
                    {
                        var task = this.YamlUtils.CalculateHashAsync(new Uri(this.Url.CurrentValue), cts.Token, progress);
                        this.HashingProgress.MonitorProgress(task, cts, progress);
                        var newHash = await task.ConfigureAwait(false);

                        // this is to make sure that the hash is uppercase or lowercase depending on the source. We prefer lowercase
                        if (true == this.Sha256.CurrentValue?.All(c => char.IsUpper(c) || char.IsDigit(c)))
                        {
                            newHash = newHash.ToUpperInvariant();
                        }
                        else
                        {
                            newHash = newHash.ToLowerInvariant();
                        }

                        this.Sha256.CurrentValue = newHash;
                    }
                    else if (this.IsMsix && parameter == "signature")
                    {
                        var task = this.YamlUtils.CalculateSignatureHashAsync(new Uri(this.Url.CurrentValue), cts.Token, progress);
                        this.HashingProgressSignature.MonitorProgress(task, cts, progress);
                        var newHash = await task.ConfigureAwait(false);

                        // this is to make sure that the hash is uppercase or lowercase depending on the source. We prefer lowercase
                        if (true == this.SignatureSha256.CurrentValue?.All(c => char.IsUpper(c) || char.IsDigit(c)))
                        {
                            newHash = newHash.ToUpperInvariant();
                        }
                        else
                        {
                            newHash = newHash.ToLowerInvariant();
                        }

                        this.SignatureSha256.CurrentValue = newHash;
                    }
                }
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e);
            }
        }

        private async void OpenHash(string parameter)
        {
            if (!this.interactionService.SelectFile(out var path))
            {
                return;
            }
         
            var progress = new Progress();
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    if (parameter == "installer")
                    {
                        var task = this.YamlUtils.CalculateHashAsync(new FileInfo(path), cts.Token, progress);
                        this.HashingProgress.MonitorProgress(task, cts, progress);
                        this.Sha256.CurrentValue = await task.ConfigureAwait(false);
                    }
                    else if (this.IsMsix && parameter == "signature")
                    {
                        var task = this.YamlUtils.CalculateSignatureHashAsync(new FileInfo(path), cts.Token, progress);
                        this.HashingProgressSignature.MonitorProgress(task, cts, progress);
                        this.SignatureSha256.CurrentValue = await task.ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    this.interactionService.ShowError($"The file could not be hashed. {e.Message}", e);
                }
            }
        }

        public ProgressProperty HashingProgress { get; } = new ProgressProperty();

        public ProgressProperty HashingProgressSignature { get; } = new ProgressProperty();

        public bool IsMsix => this.InstallerType.CurrentValue == YamlInstallerType.msix || this.InstallerType.CurrentValue == YamlInstallerType.appx;

        public bool IsCommand
        {
            get
            {
                switch (this.InstallerType.CurrentValue)
                {
                    case YamlInstallerType.none:
                    case YamlInstallerType.exe:
                        return true;
                    default:
                        return false;
                }
            }
        }
        
        public override void Commit()
        {
            base.Commit();

            this.Model.Url = this.Url.CurrentValue;
            this.Model.SystemAppId = this.SystemAppId.CurrentValue;

            if (this.Architecture.CurrentValue == YamlArchitecture.none)
            {
                this.Model.Arch = null;
            }
            else
            {
                this.Model.Arch = this.Architecture.CurrentValue;
            }

            if (this.Scope.CurrentValue == YamlScope.none)
            {
                this.Model.Scope = null;
            }
            else
            {
                this.Model.Scope = this.Scope.CurrentValue;
            }

            if (string.IsNullOrEmpty(this.SilentCommandWithProgress.CurrentValue) && string.IsNullOrEmpty(this.SilentCommand.CurrentValue))
            {
                this.Model.Switches = null;
            }
            else if (this.Model.Switches == null)
            {
                this.Model.Switches = new YamlSwitches();
            }

            if (this.Model.Switches != null)
            {
                this.Model.Switches.SilentWithProgress = this.SilentCommandWithProgress.CurrentValue;
                this.Model.Switches.Silent = this.SilentCommand.CurrentValue;
                this.Model.Switches.Custom = this.CustomCommand.CurrentValue;
            }

            this.Model.Sha256 = this.Sha256.CurrentValue;
            this.Model.SystemAppId = this.SystemAppId.CurrentValue;
            this.Model.SignatureSha256 = this.SignatureSha256.CurrentValue;
        }
        private string ValidateSystemAppId(string value)
        {
            return null;
            // This must not return errors, because CLI of winget does not let this value...
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            switch (this.InstallerType.CurrentValue)
            {
                case YamlInstallerType.msi:
                    if (!Guid.TryParse(value, out _))
                    {
                        return $"Value {value} is not a valid GUID.";
                    }

                    break;
            }

            return null;
        }

        private void InstallerTypeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsCommand));
            this.OnPropertyChanged(nameof(IsMsix));
        }
        
        private string ValidateInstallerType(YamlInstallerType value)
        {
            if (value == YamlInstallerType.none)
            {
                return "The installation type must be selected.";
            }

            return null;
        }
    }
}