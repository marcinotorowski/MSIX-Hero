using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Winget.Yaml;
using Otor.MsixHero.Winget.Yaml.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Winget.YamlEditor.ViewModel
{
    public class WingetInstallerViewModel : ChangeableContainer
    {
        private readonly YamlUtils yamlUtils;
        private readonly IInteractionService interactionService;
        private ICommand generateSha256, openSha256;

        public WingetInstallerViewModel(YamlUtils yamlUtils, IInteractionService interactionService)
        {
            this.yamlUtils = yamlUtils;
            this.interactionService = interactionService;
            this.AddChildren(
                this.Architecture = new ChangeableProperty<YamlArchitecture>(),
                this.SystemAppId = new ValidatedChangeableProperty<string>("System AppId"),
                this.SignatureSha256 = new ValidatedChangeableProperty<string>("Signature hash", ValidatorFactory.ValidateSha256(false)),
                this.Scope = new ChangeableProperty<YamlScope?>(),
                this.SilentCommand = new ChangeableProperty<string>(),
                this.CustomCommand = new ChangeableProperty<string>(),
                this.InstallerType = new ValidatedChangeableProperty<YamlInstallerType>("Installer type", ValidateInstallerType),
                this.SilentCommandWithProgress = new ChangeableProperty<string>()
            );

            this.InstallerType.ValueChanged += InstallerTypeOnValueChanged;
        }

        public void SetData(YamlInstaller installer, bool useNullValues = true)
        {
            this.Model = installer;

            if (useNullValues || installer.Arch != null)
            {
                this.Architecture.CurrentValue = installer?.Arch ?? YamlArchitecture.none;
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
        }

        public ChangeableProperty<YamlArchitecture> Architecture { get; }

        public ChangeableProperty<YamlInstallerType> InstallerType { get; }

        public ChangeableProperty<YamlScope?> Scope { get; }

        public ChangeableProperty<string> SystemAppId { get; }

        public ValidatedChangeableProperty<string> SignatureSha256 { get; }

        public ChangeableProperty<string> SilentCommand { get; }

        public ChangeableProperty<string> CustomCommand { get; }

        public ChangeableProperty<string> SilentCommandWithProgress { get; }
        
        public YamlInstaller Model { get; set; }

        public string Url { get; set; }

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

        public ICommand GenerateSha256
        {
            get
            {
                return this.generateSha256 ??= new DelegateCommand<string>(this.GenerateHash);
            }
        }

        public ProgressProperty HashingProgressSignature { get; } = new ProgressProperty();

        public ICommand OpenSha256
        {
            get
            {
                return this.openSha256 ??= new DelegateCommand<string>(this.OpenHash);
            }
        }

        private async void GenerateHash(string parameter)
        {
            if (string.IsNullOrEmpty(this.Url))
            {
                this.interactionService.ShowError("You must first configure the installer URL before a hash can be calculated.");
                return;
            }

            if (this.interactionService.Confirm($"This will download the file '{this.Url}' and calculate its hash. The download may take a while, do you want to continue?", type: InteractionType.Question, buttons: InteractionButton.YesNo) == InteractionResult.No)
            {
                return;
            }

            var progress = new Progress();
            try
            {
                if (this.IsMsix)
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        var task = this.yamlUtils.CalculateSignatureHashAsync(new Uri(this.Url), cts.Token, progress);
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
                    if (this.IsMsix)
                    {
                        var task = this.yamlUtils.CalculateSignatureHashAsync(new FileInfo(path), cts.Token, progress);
                        this.HashingProgressSignature.MonitorProgress(task, cts, progress);
                        this.SignatureSha256.CurrentValue = await task.ConfigureAwait(true);
                    }
                }
                catch (Exception e)
                {
                    this.interactionService.ShowError($"The file could not be hashed. {e.Message}", e);
                }
            }
        }

        public override void Commit()
        {
            base.Commit();

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

            this.Model.SystemAppId = this.SystemAppId.CurrentValue;
            this.Model.SignatureSha256 = this.SignatureSha256.CurrentValue;
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