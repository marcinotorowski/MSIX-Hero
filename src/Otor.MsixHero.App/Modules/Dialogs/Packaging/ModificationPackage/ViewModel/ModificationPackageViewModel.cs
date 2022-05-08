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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Editor;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;
using LogManager = Otor.MsixHero.Infrastructure.Logging.LogManager;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.ModificationPackage.ViewModel
{
    public class ModificationPackageViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private static readonly LogSource Logger = new();        private readonly IModificationPackageBuilder _contentBuilder;
        private readonly IUacElevation _uacElevation;
        private readonly IConfigurationService _configurationService;
        private readonly IInteractionService _interactionService;
        private readonly ITimeStampFeed _timeStampFeed;
        private ICommand _openSuccessLink;
        private ICommand _reset;
        
        public ModificationPackageViewModel(
            IModificationPackageBuilder contentBuilder,
            IUacElevation uacElevation,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            ITimeStampFeed timeStampFeed) : base("Create modification package", interactionService)
        {
            this._contentBuilder = contentBuilder;
            this._uacElevation = uacElevation;
            this._configurationService = configurationService;
            this._interactionService = interactionService;
            this._timeStampFeed = timeStampFeed;

            this.InitializeTabProperties();
            this.InitializeTabParentPackage();
            this.InitializeTabContent();
            this.InitializeTabCertificate();

            this.AddChildren(
                this.TabProperties,
                this.TabParentPackage,
                this.TabContent,
                this.TabCertificate);
        }

        public ChangeableFileProperty SourcePath { get; private set; }

        public ValidatedChangeableProperty<string> ParentPublisher { get; private set; }
        
        public ValidatedChangeableProperty<string> ParentName { get; private set; }

        public ChangeableContainer TabProperties { get; private set; }

        public ValidatedChangeableProperty<string> Name { get; private set; }

        public ValidatedChangeableProperty<string> PublisherName { get; private set; }

        public ValidatedChangeableProperty<string> DisplayName { get; private set; }

        public ValidatedChangeableProperty<string> PublisherDisplayName { get; private set; }

        public ValidatedChangeableProperty<string> Version { get; private set; }

        public ChangeableProperty<bool> IncludeFiles { get; private set; }

        public ChangeableContainer TabParentPackage { get; private set; }

        public ChangeableContainer TabContent { get; private set; }

        public ChangeableProperty<PackageSourceMode> PackageSourceMode { get; private set; }

        public ChangeableProperty<bool> IncludeRegistry { get; private set; }

        public ChangeableProperty<bool> IncludeVfsFolders { get; private set; }

        public ChangeableFileProperty SourceRegistryFile { get; private set; }

        public ChangeableFolderProperty SourceFolder { get; private set; }

        public ChangeableProperty<ModificationPackageBuilderAction> Create { get; private set; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this._openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this._reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public string Result { get; private set; }

        public bool IsIncludeVfsFoldersEnabled => !string.IsNullOrEmpty(this.SourcePath.CurrentValue) && this.PackageSourceMode.CurrentValue == ViewModel.PackageSourceMode.FromFile && this.Create.CurrentValue == ModificationPackageBuilderAction.Manifest;

        public CertificateSelectorViewModel TabCertificate { get; private set; }
        
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (!parameters.TryGetValue("file", out string sourceFile))
            {
                return;
            }

            this.SourcePath.CurrentValue = sourceFile;
            this.TabProperties.Commit();
            this.TabParentPackage.Commit();
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            // ReSharper disable once NotAccessedVariable
            string selectedPath;

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    if (!this._interactionService.SelectFolder(out selectedPath))
                    {
                        return false;
                    }

                    selectedPath = Path.Join(selectedPath, FileConstants.AppxManifestFile);
                    break;

                case ModificationPackageBuilderAction.Msix:
                case ModificationPackageBuilderAction.SignedMsix:
                    if (!this._interactionService.SaveFile(FileDialogSettings.FromFilterString("MSIX Modification Packages|*" + FileConstants.MsixExtension), out selectedPath))
                    {
                        return false;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var modificationPkgCreationRequest = new ModificationPackageConfig
            {
                DisplayName = this.DisplayName.CurrentValue,
                Name = this.Name.CurrentValue,
                Publisher = this.PublisherName.CurrentValue,
                DisplayPublisher = this.PublisherDisplayName.CurrentValue,
                Version = this.Version.CurrentValue,
                ParentName = this.ParentName.CurrentValue,
                ParentPublisher = this.ParentPublisher.CurrentValue,
                IncludeVfsFolders = this.IncludeVfsFolders.CurrentValue && this.IsIncludeVfsFoldersEnabled,
                IncludeFolder = this.IncludeFiles.CurrentValue && !string.IsNullOrEmpty(this.SourceFolder.CurrentValue) ? new DirectoryInfo(this.SourceFolder.CurrentValue) : null,
                IncludeRegistry = this.IncludeRegistry.CurrentValue && !string.IsNullOrEmpty(this.SourceRegistryFile.CurrentValue) ? new FileInfo(this.SourceRegistryFile.CurrentValue) : null,
                ParentPackagePath = this.SourcePath.CurrentValue
            };

            await this._contentBuilder.Create(modificationPkgCreationRequest, selectedPath, this.Create.CurrentValue, cancellationToken, progress).ConfigureAwait(false);

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    this.Result = selectedPath;
                    break;
                case ModificationPackageBuilderAction.Msix:
                    this.Result = selectedPath;
                    break;
                case ModificationPackageBuilderAction.SignedMsix:

                    var manager = this._uacElevation.AsCurrentUser<ISigningManager>();
                    cancellationToken.ThrowIfCancellationRequested();

                    string timeStampUrl;
                    switch (this.TabCertificate.TimeStampSelectionMode.CurrentValue)
                    {
                        case TimeStampSelectionMode.None:
                            timeStampUrl = null;
                            break;
                        case TimeStampSelectionMode.Auto:
                            timeStampUrl = "auto";
                            break;
                        case TimeStampSelectionMode.Url:
                            timeStampUrl = this.TabCertificate.TimeStamp.CurrentValue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (this.TabCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Pfx:
                            await manager.SignPackageWithPfx(selectedPath, true, this.TabCertificate.PfxPath.CurrentValue, this.TabCertificate.Password.CurrentValue, timeStampUrl, IncreaseVersionMethod.None, cancellationToken, progress).ConfigureAwait(false);
                            break;
                        case CertificateSource.Personal:
                            await manager.SignPackageWithInstalled(selectedPath, true, this.TabCertificate.SelectedPersonalCertificate?.CurrentValue?.Model, timeStampUrl, IncreaseVersionMethod.None,cancellationToken, progress).ConfigureAwait(false);
                            break;
                        case CertificateSource.DeviceGuard:
                            await manager.SignPackageWithDeviceGuardFromUi(selectedPath, this.TabCertificate.DeviceGuard.CurrentValue, timeStampUrl, IncreaseVersionMethod.None,cancellationToken, progress).ConfigureAwait(false);
                            break;
                    }

                    this.Result = selectedPath;
                    break;
            }

            return true;
        }

        private void ResetExecuted()
        {
            this.State.IsSaved = false;
        }

        private void CreateOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.TabCertificate.IsValidated = this.Create.CurrentValue == ModificationPackageBuilderAction.SignedMsix;
            if (this.Create.CurrentValue != ModificationPackageBuilderAction.Manifest)
            {
                this.IncludeVfsFolders.CurrentValue = false;
            }
            
            this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
        }

        private void PackageSourceModeChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
            this.ParentName.IsValidated = (PackageSourceMode)e.NewValue == ViewModel.PackageSourceMode.FromProperties;
            this.ParentPublisher.IsValidated = (PackageSourceMode)e.NewValue == ViewModel.PackageSourceMode.FromProperties;
            this.SourcePath.IsValidated = (PackageSourceMode)e.NewValue == ViewModel.PackageSourceMode.FromFile;
        }

        private void OpenSuccessLinkExecuted()
        {
            Process.Start("explorer.exe", "/select," + this.Result);
        }

        private void IncludeFilesChanged(object sender, ValueChangedEventArgs e)
        {
            this.SourceFolder.IsValidated = (bool)e.NewValue;
        }

        private void IncludeRegistryChanged(object sender, ValueChangedEventArgs e)
        {
            this.SourceRegistryFile.IsValidated = (bool)e.NewValue;
        }
        private void InitializeTabProperties()
        {
            this.DisplayName = new ValidatedChangeableProperty<string>("Displayed name", ValidatorFactory.ValidateNotEmptyField());
            this.PublisherDisplayName = new ValidatedChangeableProperty<string>("Displayed publisher name", ValidatorFactory.ValidateNotEmptyField());
            this.Name = new ValidatedChangeableProperty<string>("Package name", AppxValidatorFactory.ValidatePackageName());
            this.PublisherName = new ValidatedChangeableProperty<string>("Publisher name", AppxValidatorFactory.ValidateSubject());
            this.Version = new ValidatedChangeableProperty<string>("Version", "1.0.0.0", AppxValidatorFactory.ValidateVersion());

            this.TabProperties = new ChangeableContainer(
                this.DisplayName,
                this.PublisherDisplayName,
                this.Name,
                this.PublisherName,
                this.Version);

            this.TabProperties.Commit();

            this.DisplayName.ValueChanged += DisplayNameOnValueChanged;
            this.PublisherDisplayName.ValueChanged += PublisherDisplayNameOnValueChanged;
        }

        private void DisplayNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.Name.IsTouched || string.IsNullOrEmpty((string)e.NewValue))
            {
                return;
            }

            this.Name.CurrentValue = Regex.Replace((string) e.NewValue, "[^a-zA-Z0-9\\-]", string.Empty);
            this.Name.Commit();
        }

        private void PublisherDisplayNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.PublisherName.IsTouched || string.IsNullOrEmpty((string)e.NewValue))
            {
                return;
            }

            this.PublisherName.CurrentValue = "CN=" + Regex.Replace((string) e.NewValue, "[,=]", string.Empty);
            this.PublisherName.Commit();
        }

        private void InitializeTabParentPackage()
        {
            this.SourcePath = new ChangeableFileProperty("Parent package path", this._interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                // ReSharper disable once StringLiteralTypo
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile).BuildFilter(),
                IsValidated = true
            };

            this.PackageSourceMode = new ChangeableProperty<PackageSourceMode>();
            this.ParentName = new ValidatedChangeableProperty<string>("Parent package name", false, AppxValidatorFactory.ValidatePackageName());
            this.ParentPublisher = new ValidatedChangeableProperty<string>("Parent publisher", false, AppxValidatorFactory.ValidateSubject());
            
            this.TabParentPackage = new ChangeableContainer(
                this.PackageSourceMode,
                this.ParentName,
                this.ParentPublisher,
                this.SourcePath);

            this.SourcePath.ValueChanged += SourcePathOnValueChanged;
            this.PackageSourceMode.ValueChanged += this.PackageSourceModeChanged;
        }

        private void SourcePathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                using IAppxFileReader reader = FileReaderFactory.CreateFileReader((string) e.NewValue);
                var mr = new AppxManifestReader();
                var read = mr.Read(reader).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(this.DisplayName.CurrentValue))
                {
                    this.DisplayName.CurrentValue = read.DisplayName + " - Modification package";
                }

                this.ParentName.CurrentValue = read.Name;
                this.ParentPublisher.CurrentValue = read.Publisher;

                this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
            }
            catch (Exception exception)
            {
                Logger.Error().WriteLine(exception);
                this._interactionService.ShowError("Could not read the properties from the package.", exception);
            }
        }

        private void InitializeTabContent()
        {
            this.Create = new ChangeableProperty<ModificationPackageBuilderAction>();
            if (_configurationService.GetCurrentConfiguration().Packer.SignByDefault)
            {
                this.Create.CurrentValue = ModificationPackageBuilderAction.SignedMsix;
            }

            this.IncludeFiles = new ChangeableProperty<bool>();
            this.IncludeRegistry = new ChangeableProperty<bool>();
            this.IncludeVfsFolders = new ChangeableProperty<bool>();

            this.SourceFolder = new ChangeableFolderProperty("Folder to include", this._interactionService, ChangeableFolderProperty.ValidatePathAndPresence)
            {
                IsValidated = false
            };

            this.SourceRegistryFile = new ChangeableFileProperty(".REG file to include", this._interactionService, ChangeableFileProperty.ValidatePathAndPresence)
            {
                IsValidated = false,
                Filter = new DialogFilterBuilder("*.reg").BuildFilter(),
                OpenForSaving = false
            };

            this.TabContent = new ChangeableContainer(
                this.Create,
                this.IncludeVfsFolders,
                this.IncludeFiles,
                this.SourceFolder,
                this.IncludeRegistry,
                this.SourceRegistryFile);

            this.Create.ValueChanged += this.CreateOnValueChanged;
            this.IncludeRegistry.ValueChanged += this.IncludeRegistryChanged;
            this.IncludeFiles.ValueChanged += this.IncludeFilesChanged;
        }

        private void InitializeTabCertificate()
        {
            this.TabCertificate = new CertificateSelectorViewModel(
                this._interactionService, 
                this._uacElevation, 
                this._configurationService.GetCurrentConfiguration()?.Signing,
                this._timeStampFeed);
        }
    }
}

