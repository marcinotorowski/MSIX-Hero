// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.App.Mvvm;
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
        private readonly YamlUtils _yamlUtils;
        private readonly IInteractionService _interactionService;
        private ICommand _generateSha256, _openSha256;

        public WingetInstallerViewModel(YamlUtils yamlUtils, IInteractionService interactionService)
        {
            this._yamlUtils = yamlUtils;
            this._interactionService = interactionService;
            this.AddChildren(
                this.Architecture = new ChangeableProperty<YamlArchitecture>(),
                this.PlatformUwp = new ChangeableProperty<bool>(this.Model?.Platform?.Contains(YamlPlatform.WindowsUniversal) == true),
                this.PlatformWin32 = new ChangeableProperty<bool>(this.Model?.Platform?.Contains(YamlPlatform.WindowsDesktop) == true),
                this.ProductCode = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_ProductCode, ValidatorFactory.ValidateGuid(false)),
                this.PackageFamilyName = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_PFN, WingetValidators.GetPackageFamilyNameError),
                this.SignatureSha256 = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_SignatureHash, ValidatorFactory.ValidateSha256(false)),
                this.Scope = new ChangeableProperty<YamlScope>(),
                this.SilentCommand = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_Silent, WingetValidators.GetInstallerSwitchesError),
                this.InteractiveCommand = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_Interactive, WingetValidators.GetInstallerSwitchesError),
                this.LogCommand = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_Log, WingetValidators.GetInstallerSwitchesError),
                this.UpgradeCommand = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_Upgrade, WingetValidators.GetInstallerSwitchesError),
                this.CustomCommand = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_Custom, WingetValidators.GetCustomInstallerSwitchesError),
                this.SilentCommandWithProgress = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Winget_Installer_SilentProgress, WingetValidators.GetInstallerSwitchesError),
                this.InstallerType = new ValidatedChangeableProperty<YamlInstallerType>(() => Resources.Localization.Dialogs_Winget_Installer_Type, ValidateInstallerType)
            );

            this.InstallerType.ValueChanged += InstallerTypeOnValueChanged;
        }

        public void SetData(YamlInstaller installer, bool useNullValues = true)
        {
            this.Model = installer;

            if (useNullValues || installer.Architecture != default)
            {
                this.Architecture.CurrentValue = installer?.Architecture ?? YamlArchitecture.None;
            }

            if (useNullValues || installer.Platform != default)
            {
                this.PlatformUwp.CurrentValue = installer?.Platform?.Contains(YamlPlatform.WindowsUniversal) == true;
                this.PlatformWin32.CurrentValue = installer?.Platform?.Contains(YamlPlatform.WindowsDesktop) == true;
            }

            if (useNullValues || installer.ProductCode != null)
            {
                this.ProductCode.CurrentValue = installer?.ProductCode;
            }

            if (useNullValues || installer.PackageFamilyName != null)
            {
                this.PackageFamilyName.CurrentValue = installer?.PackageFamilyName;
            }

            if (useNullValues || installer.SignatureSha256 != null)
            {
                this.SignatureSha256.CurrentValue = installer?.SignatureSha256;
            }

            if (useNullValues || installer.Scope != default)
            {
                this.Scope.CurrentValue = installer?.Scope ?? YamlScope.None;
            }

            if (useNullValues || installer.InstallerSwitches?.Silent != null)
            {
                this.SilentCommand.CurrentValue = installer?.InstallerSwitches?.Silent;
            }
            
            if (useNullValues || installer.InstallerSwitches?.Custom != null)
            {
                this.CustomCommand.CurrentValue = installer?.InstallerSwitches?.Custom;
            }

            if (useNullValues || installer.InstallerSwitches?.SilentWithProgress != default)
            {
                this.SilentCommandWithProgress.CurrentValue = installer?.InstallerSwitches?.SilentWithProgress;
            }
            
            if (useNullValues || installer.InstallerSwitches?.Interactive != default)
            {
                this.InteractiveCommand.CurrentValue = installer?.InstallerSwitches?.Interactive;
            }

            if (useNullValues || installer.InstallerSwitches?.Upgrade != default)
            {
                this.UpgradeCommand.CurrentValue = installer?.InstallerSwitches?.Upgrade;
            }

            if (useNullValues || installer.InstallerSwitches?.Log != default)
            {
                this.LogCommand.CurrentValue = installer?.InstallerSwitches?.Log;
            }

            if (useNullValues || installer.InstallerType != default)
            {
                this.InstallerType.CurrentValue = installer?.InstallerType ?? YamlInstallerType.None;
            }

            this.Commit();
        }

        public ChangeableProperty<YamlArchitecture> Architecture { get; }
        
        public ChangeableProperty<bool> PlatformUwp { get; }
        
        public ChangeableProperty<bool> PlatformWin32 { get; }

        public ChangeableProperty<YamlInstallerType> InstallerType { get; }

        public ChangeableProperty<YamlScope> Scope { get; }

        public ChangeableProperty<string> ProductCode { get; }

        public ChangeableProperty<string> PackageFamilyName { get; }

        public ValidatedChangeableProperty<string> SignatureSha256 { get; }

        public ChangeableProperty<string> SilentCommand { get; }
        
        public ChangeableProperty<string> InteractiveCommand { get; }
        
        public ChangeableProperty<string> LogCommand { get; }
        
        public ChangeableProperty<string> UpgradeCommand { get; }

        public ChangeableProperty<string> CustomCommand { get; }

        public ChangeableProperty<string> SilentCommandWithProgress { get; }
        
        public YamlInstaller Model { get; set; }

        public string Url { get; set; }

        public bool IsMsix => this.InstallerType.CurrentValue == YamlInstallerType.Msix || this.InstallerType.CurrentValue == YamlInstallerType.Appx;

        public bool IsCommand
        {
            get
            {
                switch (this.InstallerType.CurrentValue)
                {
                    case YamlInstallerType.None:
                    case YamlInstallerType.Exe:
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
                return this._generateSha256 ??= new DelegateCommand<string>(this.GenerateHash);
            }
        }

        public ProgressProperty HashingProgressSignature { get; } = new ProgressProperty();

        public ICommand OpenSha256
        {
            get
            {
                return this._openSha256 ??= new DelegateCommand<string>(this.OpenHash);
            }
        }

        private async void GenerateHash(string parameter)
        {
            if (string.IsNullOrEmpty(this.Url))
            {
                this._interactionService.ShowError(Resources.Localization.Dialogs_Winget_Installer_Error_MissingUrl);
                return;
            }

            if (this._interactionService.Confirm(string.Format(Resources.Localization.Dialogs_Winget_Installer_DownloadHint_Format, this.Url), type: InteractionType.Question, buttons: InteractionButton.YesNo) == InteractionResult.No)
            {
                return;
            }

            var progress = new Progress();
            try
            {
                if (!this.IsMsix)
                {
                    return;
                }

                using var cts = new CancellationTokenSource();
                var task = this._yamlUtils.CalculateSignatureHashAsync(new Uri(this.Url), progress, cts.Token);
                this.HashingProgressSignature.MonitorProgress(task, cts, progress);
                var newHash = await task.ConfigureAwait(false);
                this.SignatureSha256.CurrentValue = newHash;
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(e.Message, e);
            }
        }

        private async void OpenHash(string parameter)
        {
            if (!this._interactionService.SelectFile(out var path))
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
                        var task = this._yamlUtils.CalculateSignatureHashAsync(new FileInfo(path), cts.Token, progress);
                        this.HashingProgressSignature.MonitorProgress(task, cts, progress);
                        this.SignatureSha256.CurrentValue = await task.ConfigureAwait(true);
                    }
                }
                catch (Exception e)
                {
                    this._interactionService.ShowError(Resources.Localization.Dialogs_Winget_Installer_Error_Hash + " " + e.Message, e);
                }
            }
        }

        public override void Commit()
        {
            base.Commit();

            this.Model.PackageFamilyName = this.PackageFamilyName.CurrentValue;
            this.Model.ProductCode = this.ProductCode.CurrentValue;
            this.Model.Architecture = this.Architecture.CurrentValue;
            
            if (!this.PlatformUwp.CurrentValue && !this.PlatformWin32.CurrentValue)
            {
                this.Model.Platform = null;
            }
            else
            {
                this.Model.Platform = new List<YamlPlatform>();
                if (this.PlatformWin32.CurrentValue)
                {
                    this.Model.Platform.Add(YamlPlatform.WindowsDesktop);
                }
                
                if (this.PlatformUwp.CurrentValue)
                {
                    this.Model.Platform.Add(YamlPlatform.WindowsUniversal);
                }
            }
            
            this.Model.Scope = this.Scope.CurrentValue;

            if (
                string.IsNullOrWhiteSpace(this.SilentCommandWithProgress.CurrentValue) && 
                string.IsNullOrWhiteSpace(this.SilentCommand.CurrentValue) && 
                string.IsNullOrWhiteSpace(this.UpgradeCommand.CurrentValue) && 
                string.IsNullOrWhiteSpace(this.LogCommand.CurrentValue) && 
                string.IsNullOrWhiteSpace(this.InteractiveCommand.CurrentValue) && 
                string.IsNullOrWhiteSpace(this.CustomCommand.CurrentValue))
            {
                this.Model.InstallerSwitches = null;
            }
            else if (this.Model.InstallerSwitches == null)
            {
                this.Model.InstallerSwitches = new YamlInstallerSwitches();
            }

            if (this.Model.InstallerSwitches != null)
            {
                this.Model.InstallerSwitches.SilentWithProgress = this.SilentCommandWithProgress.CurrentValue;
                this.Model.InstallerSwitches.Silent = this.SilentCommand.CurrentValue;
                this.Model.InstallerSwitches.Custom = this.CustomCommand.CurrentValue;
                this.Model.InstallerSwitches.Interactive = this.InteractiveCommand.CurrentValue;
                this.Model.InstallerSwitches.Log = this.LogCommand.CurrentValue;
                this.Model.InstallerSwitches.Upgrade = this.UpgradeCommand.CurrentValue;
            }

            this.Model.ProductCode = this.ProductCode.CurrentValue;
            this.Model.PackageFamilyName = this.PackageFamilyName.CurrentValue;
            this.Model.SignatureSha256 = this.SignatureSha256.CurrentValue;
        }

        private void InstallerTypeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsCommand));
            this.OnPropertyChanged(nameof(IsMsix));
        }
        
        private string ValidateInstallerType(YamlInstallerType value)
        {
            if (value == YamlInstallerType.None)
            {
                return Resources.Localization.Dialogs_Winget_Installer_Error_TypeNotSet;
            }

            return null;
        }
    }
}