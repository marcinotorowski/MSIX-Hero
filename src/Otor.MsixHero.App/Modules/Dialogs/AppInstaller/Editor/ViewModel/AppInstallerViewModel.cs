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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Modules.Common.PackageSelector.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel.Tabs;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.AppInstaller;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel
{
    public enum PromptMode
    {
        Background,
        Inform,
        Force
    }
    
    public class AppInstallerViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly AppInstallerBuilder _appInstallerBuilder;
        private readonly AppInstallerFeatureSupportHelper _appInstallerFeatureSupportHelper = new AppInstallerFeatureSupportHelper();
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private ICommand _openSuccessLink;
        private ICommand _reset;
        private ICommand _open;
        private string _previousPath;
        private bool _paddingManuallyChanged;

        public AppInstallerViewModel(
            IInteractionService interactionService,
            IConfigurationService configurationService) : base(Resources.Localization.Dialogs_AppInstaller_Title, interactionService)
        {
            this._appInstallerBuilder = new AppInstallerBuilder();
            this._interactionService = interactionService;
            this._configurationService = configurationService;

            this.AppInstallerUpdateCheckingMethod = new ChangeableProperty<AppInstallerUpdateCheckingMethod>(Otor.MsixHero.AppInstaller.Entities.AppInstallerUpdateCheckingMethod.LaunchAndBackground);
            this.AllowDowngrades = new ChangeableProperty<bool>();
            this.PromptMode = new ChangeableProperty<PromptMode>();
            this.EnablePadding = new ChangeableProperty<bool>();
            this.Padding = new ValidatedChangeableProperty<string>("5000", ValidatorFactory.ValidateInteger());
            this.Padding.Changed += this.PaddingOnChanged;
            this.Version = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_AppInstaller_Version, "1.0.0.0", ValidatorFactory.ValidateVersion());
            this.MainPackageUri = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_AppInstaller_MainPackageUrl, true, ValidatorFactory.ValidateUri(true));
            this.AppInstallerUri = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_AppInstaller_AppInstallerUrl, true, ValidatorFactory.ValidateUri(true));
            this.Hours = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_AppInstaller_HoursBetweenUpdates, "24", ValidateHours);

            this.TabPackage = new PackageSelectorViewModel(
                interactionService,
                PackageSelectorDisplayMode.AllowAllPackageTypes | 
                PackageSelectorDisplayMode.ShowTypeSelector | 
                PackageSelectorDisplayMode.AllowManifests | 
                PackageSelectorDisplayMode.ShowActualName | 
                PackageSelectorDisplayMode.RequireFullIdentity |
                PackageSelectorDisplayMode.AllowBrowsing | 
                PackageSelectorDisplayMode.AllowChanging)
            {
                CustomPrompt = Resources.Localization.Dialogs_AppInstaller_Target
            };

            this.TabOptions = new ChangeableContainer(
                this.AppInstallerUpdateCheckingMethod,
                this.AllowDowngrades,
                this.PromptMode,
                this.Hours);

            this.TabAdvanced = new ChangeableContainer(
                this.EnablePadding,
                this.Padding);

            this.AddChildren(
                this.TabPackage,
                this.TabProperties = new ChangeableContainer(this.Version, this.MainPackageUri, this.AppInstallerUri),
                this.TabOptionalPackages = new AppInstallerPackagesViewModel(this._interactionService, this._configurationService),
                this.TabDependencies = new AppInstallerPackagesViewModel(this._interactionService, this._configurationService),
                this.TabRelatedPackages = new AppInstallerPackagesViewModel(this._interactionService, this._configurationService),
                this.TabOptions,
                this.TabAdvanced);

            this.TabPackage.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.AppInstallerUpdateCheckingMethod.ValueChanged += this.AppInstallerUpdateCheckingMethodValueChanged;
            this.AllowDowngrades.ValueChanged += this.OnCompatRelevantPropertyChanged;
            this.PromptMode.ValueChanged += this.OnCompatRelevantPropertyChanged;
        }

        private void PaddingOnChanged(object sender, EventArgs e)
        {
            this._paddingManuallyChanged = true;
        }

        public ChangeableContainer TabProperties { get; }
        
        public AppInstallerPackagesViewModel TabOptionalPackages { get; }
        
        public AppInstallerPackagesViewModel TabDependencies { get; }
        
        public AppInstallerPackagesViewModel TabRelatedPackages { get; }

        public ChangeableContainer TabOptions { get; }
        
        public ChangeableContainer TabAdvanced { get; }

        public bool ShowLaunchOptions =>
            this.AppInstallerUpdateCheckingMethod.CurrentValue == Otor.MsixHero.AppInstaller.Entities.AppInstallerUpdateCheckingMethod.LaunchAndBackground ||
            this.AppInstallerUpdateCheckingMethod.CurrentValue == Otor.MsixHero.AppInstaller.Entities.AppInstallerUpdateCheckingMethod.Launch;

        public ChangeableProperty<AppInstallerUpdateCheckingMethod> AppInstallerUpdateCheckingMethod { get; }

        public ValidatedChangeableProperty<string> Hours { get; }
        
        public ChangeableProperty<bool> EnablePadding { get; }

        public ChangeableProperty<string> Padding { get; }

        public ChangeableProperty<PromptMode> PromptMode { get; }

        public ValidatedChangeableProperty<string> Version { get; }

        public ChangeableProperty<bool> AllowDowngrades { get; }
        
        public ValidatedChangeableProperty<string> MainPackageUri { get; }

        public ValidatedChangeableProperty<string> AppInstallerUri { get; }

        public string CompatibleWindows
        {
            get
            {
                var minWin10 = this._appInstallerFeatureSupportHelper.GetLowestSupportedWindows10Build(this.GetCurrentAppInstallerConfig());
                return $"Windows 10 {minWin10}";
            }
        }
        
        public ICommand OpenSuccessLinkCommand
        {
            get { return this._openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this._reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ICommand OpenCommand
        {
            get { return this._open ??= new DelegateCommand<string>(this.OpenExecuted); }
        }

        public PackageSelectorViewModel TabPackage { get; }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("file", out string sourceFile))
            {
                var ext = Path.GetExtension(sourceFile) ?? string.Empty;

                switch (ext.ToLowerInvariant())
                {
                    case FileConstants.AppInstallerExtension:
                        this.OpenCommand.Execute(sourceFile);
                        break;
                    default:
                        this.TabPackage.InputPath.CurrentValue = sourceFile;
                        this.TabPackage.AllowChangingSourcePackage = false;
                        this.TabPackage.ShowPackageTypeSelector = false;
                        break;
                }
            }
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            var settings = new FileDialogSettings(new DialogFilterBuilder().WithAll().WithAppInstaller(), this._previousPath);
            if (!this._interactionService.SaveFile(settings, out var selected))
            {
                return false;
            }

            if (this.EnablePadding.CurrentValue)
            {
                // re-calculate the manual padding in case the user did not touch the field
                await this.CalculatePadding().ConfigureAwait(false);
            }

            this._previousPath = selected;
            var appInstaller = this.GetCurrentAppInstallerConfig();
            await this._appInstallerBuilder.Create(appInstaller, selected, cancellationToken, progress).ConfigureAwait(false);

            var sizeHelper = new AppInstallerSizeInfo(new FileInfo(selected));
            if (this.EnablePadding.CurrentValue && int.TryParse(this.Padding.CurrentValue, out var parsedSize))
            {
                await sizeHelper.Pad(parsedSize);
            }

            return true;
        }

        private static string ValidateHours(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return Resources.Localization.Dialogs_AppInstaller_Validation_Hours_Empty;
            }

            if (!int.TryParse(newValue, out var value))
            {
                return string.Format(Resources.Localization.Dialogs_AppInstaller_Validation_Hours_NaN, newValue);
            }

            if (value < 0 || value > 255)
            {
                return string.Format(Resources.Localization.Dialogs_AppInstaller_Validation_Hours_Range, newValue);
            }

            return null;
        }

        private AppInstallerConfig GetCurrentAppInstallerConfig()
        {
            var prompt = this.PromptMode.CurrentValue;
            
            var builder = new AppInstallerBuilder
            {
                Version = this.Version.CurrentValue,
                MainPackageType = this.TabPackage.PackageType.CurrentValue,
                MainPackageName = this.TabPackage.Name.CurrentValue,
                MainPackageArchitecture = this.TabPackage.Architecture.CurrentValue,
                MainPackagePublisher = this.TabPackage.Publisher.CurrentValue,
                MainPackageVersion = this.TabPackage.Version.CurrentValue,
                HoursBetweenUpdateChecks = int.Parse(this.Hours.CurrentValue),
                CheckForUpdates = this.AppInstallerUpdateCheckingMethod.CurrentValue,
                ShowPrompt = prompt != ViewModel.PromptMode.Background,
                UpdateBlocksActivation = prompt == ViewModel.PromptMode.Force,
                AllowDowngrades = this.AllowDowngrades.CurrentValue,
                RedirectUri = string.IsNullOrEmpty(this.AppInstallerUri.CurrentValue) ? null : new Uri(this.AppInstallerUri.CurrentValue),
                MainPackageUri = string.IsNullOrEmpty(this.MainPackageUri.CurrentValue) ? null : new Uri(this.MainPackageUri.CurrentValue),
            };

            var appInstaller = builder.Build();

            if (this.TabOptionalPackages.Items.Any() && appInstaller.Optional == null)
            {
                appInstaller.Optional = new List<AppInstallerBaseEntry>();
            }

            if (this.TabDependencies.Items.Any() && appInstaller.Dependencies == null)
            {
                appInstaller.Dependencies = new List<AppInstallerBaseEntry>();
            }

            if (this.TabRelatedPackages.Items.Any() && appInstaller.Related == null)
            {
                appInstaller.Related = new List<AppInstallerBaseEntry>();
            }
            
            foreach (var optional in this.TabOptionalPackages.Items)
            {
                var model = optional.ToModel();
                appInstaller.Optional.Add(model);
            }
            
            foreach (var dependency in this.TabDependencies.Items)
            {
                var model = dependency.ToModel();
                appInstaller.Dependencies.Add(model);
            }
            
            foreach (var related in this.TabRelatedPackages.Items)
            {
                var model = related.ToModel();
                appInstaller.Related.Add(model);
            }
            
            return appInstaller;
        }

        private void ResetExecuted()
        {
            this.State.IsSaved = false;
        }

        private async void OpenExecuted(string selected)
        {
            if (selected != null)
            {
                if (!File.Exists(selected))
                {
                    selected = null;
                }
                else
                {
                    _previousPath = selected;
                }
            }

            if (selected == null)
            {
                if (this._previousPath != null)
                {
                    var settings = new FileDialogSettings(new DialogFilterBuilder().WithAppInstaller().WithAll(), this._previousPath);
                    if (!this._interactionService.SelectFile(settings, out selected))
                    {
                        return;
                    }
                }
                else if (!this._interactionService.SelectFile(FileDialogSettings.FromFilterString(new DialogFilterBuilder().WithAppInstaller().WithAll()), out selected))
                {
                    return;
                }
            }
            
            this._previousPath = selected;

            AppInstallerConfig file;
            
            try
            {
                file = await AppInstallerConfig.FromFile(selected).ConfigureAwait(true);

                var sizeHelper = new AppInstallerSizeInfo(new FileInfo(selected));
                this.EnablePadding.CurrentValue = await sizeHelper.IsPadded().ConfigureAwait(true);

                if (this.EnablePadding.CurrentValue)
                {
                    this.Padding.CurrentValue = sizeHelper.FileSize.ToString("0");
                }
                else
                {
                    this.Padding.CurrentValue = "0";
                }
            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleNullReferenceException
                this._interactionService.ShowError(Resources.Localization.Dialogs_AppInstaller_Validation_File_Format, e, InteractionResult.OK);
                return;
            }
            
            var builder = new AppInstallerBuilder(file);

            this.AllowDowngrades.CurrentValue = builder.AllowDowngrades;
            this.AppInstallerUpdateCheckingMethod.CurrentValue = builder.CheckForUpdates;
            this.AppInstallerUri.CurrentValue = builder.RedirectUri.ToString();
            
            if (builder.ShowPrompt)
            {
                this.PromptMode.CurrentValue = builder.UpdateBlocksActivation ? ViewModel.PromptMode.Force : ViewModel.PromptMode.Inform;
            }
            else
            {
                this.PromptMode.CurrentValue = ViewModel.PromptMode.Background;
            }
            
            this.Hours.CurrentValue = builder.HoursBetweenUpdateChecks.ToString();
            this.MainPackageUri.CurrentValue = builder.MainPackageUri.ToString();
            this.Version.CurrentValue = builder.Version;

            this.TabPackage.Name.CurrentValue = builder.MainPackageName;
            this.TabPackage.Version.CurrentValue = builder.MainPackageVersion;
            this.TabPackage.Publisher.CurrentValue = builder.MainPackagePublisher;
            this.TabPackage.PackageType.CurrentValue = builder.MainPackageType;
            this.TabPackage.Architecture.CurrentValue = builder.MainPackageArchitecture;
            this.TabOptionalPackages.SetPackages(file.Optional);
            this.TabDependencies.SetPackages(file.Dependencies);
            this.TabRelatedPackages.SetPackages(file.Related);

            this.AllowDowngrades.CurrentValue = builder.AllowDowngrades;

            this.OnPropertyChanged(nameof(ShowLaunchOptions));
            this.OnPropertyChanged(nameof(CompatibleWindows));
        }

        private void OpenSuccessLinkExecuted()
        {
            Process.Start("explorer.exe", "/select," + _previousPath);
        }
        
        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)e.NewValue))
            {
                var isManifest = string.Equals(Path.GetFileName((string)e.NewValue), FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase);
                
                if (isManifest)
                {
                    return;
                }
            }
            
            if (string.IsNullOrEmpty(this.MainPackageUri.CurrentValue) && !string.IsNullOrEmpty(e.NewValue as string))
            {
                var newFilePath = new FileInfo((string)e.NewValue);
                var configValue = this._configurationService.GetCurrentConfiguration().AppInstaller?.DefaultRemoteLocationPackages;
                if (string.IsNullOrEmpty(configValue))
                {
                    configValue = "http://server-name/";
                }

                this.MainPackageUri.CurrentValue = $"{configValue.TrimEnd('/')}/{newFilePath.Name}";
            }
            
            if (string.IsNullOrEmpty(this.AppInstallerUri.CurrentValue) && !string.IsNullOrEmpty(e.NewValue as string))
            {
                var newFilePath = new FileInfo((string)e.NewValue);
                var configValue = this._configurationService.GetCurrentConfiguration().AppInstaller?.DefaultRemoteLocationPackages;
                if (string.IsNullOrEmpty(configValue))
                {
                    configValue = "http://server-name/";
                }

                var newName = Path.ChangeExtension(newFilePath.Name, FileConstants.AppInstallerExtension);
                this.AppInstallerUri.CurrentValue = $"{configValue.TrimEnd('/')}/{newName}";
            }
        }

        private void AppInstallerUpdateCheckingMethodValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(ShowLaunchOptions));
            this.OnPropertyChanged(nameof(CompatibleWindows));
        }

        private void OnCompatRelevantPropertyChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(CompatibleWindows));
        }

        public async Task CalculatePadding()
        {
            this.CurrentSize = await AppInstallerSizeInfo.GetActualSize(this.GetCurrentAppInstallerConfig()).ConfigureAwait(false);
            this.OnPropertyChanged(nameof(CurrentSize));

            if (this._paddingManuallyChanged)
            {
                return;
            }

            var wasTouched = this.Padding.IsTouched;

            this.Padding.CurrentValue = AppInstallerSizeInfo.GetSuggestedPaddedSize(this.CurrentSize).ToString("0");
            this._paddingManuallyChanged = false;
            
            if (!wasTouched)
            {
                this.Padding.Commit();
            }
        }

        public int CurrentSize { get; private set; }
    }
}