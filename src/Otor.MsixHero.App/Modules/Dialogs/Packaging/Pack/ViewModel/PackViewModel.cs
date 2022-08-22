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
using System.Xml.Linq;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Modules.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Editor.Facades;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.ManifestCreator;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel
{
    public class PackViewModel : ChangeableDialogViewModel
    {
        private readonly IUacElevation _uacElevation;
        private readonly IAppxManifestCreator _manifestCreator;
        private ICommand _openSuccessLink;
        private ICommand _resetCommand;

        public PackViewModel(
            IUacElevation uacElevation,
            IAppxManifestCreator manifestCreator,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            ITimeStampFeed timeStampFeed) : base(Resources.Localization.Dialogs_Pack_Title, interactionService)
        {
            this._uacElevation = uacElevation;
            this._manifestCreator = manifestCreator;
            var signConfig = configurationService.GetCurrentConfiguration().Signing ?? new SigningConfiguration();
            var signByDefault = configurationService.GetCurrentConfiguration().Packer?.SignByDefault == true;

            this.InputPath = new ChangeableFolderProperty(() => Resources.Localization.Dialogs_Pack_SrcDir, interactionService, ChangeableFolderProperty.ValidatePath);

            this.OutputPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Pack_TargetPackagePath, interactionService, ChangeableFileProperty.ValidatePath)
            {
                OpenForSaving = true,
                Filter = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension).BuildFilter()
            };

            this.Sign = new ChangeableProperty<bool>(signByDefault);
            this.Compress = new ChangeableProperty<bool>(true);
            this.Validate = new ChangeableProperty<bool>(true);
            this.RemoveDirectory = new ChangeableProperty<bool>();
            this.OverrideSubject = new ChangeableProperty<bool>(true);

            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, uacElevation, signConfig, timeStampFeed)
            {
                IsValidated = false
            };

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;
            this.Sign.ValueChanged += this.SignOnValueChanged;

            this.TabSource = new ChangeableContainer(this.InputPath, this.OutputPath, this.Sign, this.Compress, this.Validate, this.RemoveDirectory);
            this.TabSigning = new ChangeableContainer(this.SelectedCertificate, this.OverrideSubject);
            
            this.AddChildren(this.TabSource, this.TabSigning);
        }

        public ChangeableContainer TabSource { get; }

        public ChangeableContainer TabSigning { get; }

        public ChangeableFileProperty OutputPath { get; }

        public ChangeableProperty<bool> OverrideSubject { get; }

        public ChangeableFolderProperty InputPath { get; }

        public ICommand OpenSuccessLink
        {
            get { return this._openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this._resetCommand ??= new DelegateCommand(this.ResetExecuted); }
        }

        public CertificateSelectorViewModel SelectedCertificate { get; }

        public ChangeableProperty<bool> Sign { get; }

        public ChangeableProperty<bool> Compress { get; }

        public ChangeableProperty<bool> Validate { get; }

        public ChangeableProperty<bool> RemoveDirectory { get; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var temporaryFiles = new List<string>();
            try
            {
                var fileListBuilder = new PackageFileListBuilder();
                fileListBuilder.AddDirectory(this.InputPath.CurrentValue, true, null);

                if (this.PrePackOptions != null && !this.PrePackOptions.ManifestPresent)
                {
                    if (!this.PrePackOptions.CanConvert)
                    {
                        throw new InvalidOperationException(Resources.Localization.Dialogs_Pack_NoManifests);
                    }

                    if (!string.IsNullOrEmpty(this.InputPath.CurrentValue))
                    {
                        progress.Report(new ProgressData(0, Resources.Localization.Dialogs_Pack_CreatingManifest));
                        var options = new AppxManifestCreatorOptions
                        {
                            CreateLogo = this.PrePackOptions.CreateLogo,
                            EntryPoints = this.PrePackOptions.EntryPoints.Where(e => e.IsChecked).Select(e => e.Value).ToArray(),
                            PackageDisplayName = Path.GetFileName(this.InputPath.CurrentValue),
                            RegistryFile = this.PrePackOptions.SelectedRegistry?.FilePath == null ? null : new FileInfo(this.PrePackOptions.SelectedRegistry.FilePath)
                        };

                        // ReSharper disable once AssignNullToNotNullAttribute
                        await foreach(var result in this._manifestCreator.CreateManifestForDirectory(new DirectoryInfo(this.InputPath.CurrentValue), options, cancellationToken).ConfigureAwait(false))
                        {
                            temporaryFiles.Add(result.SourcePath);

                            if (result.PackageRelativePath == null)
                            {
                                continue;
                            }
                            
                            fileListBuilder.AddFile(result.SourcePath, result.PackageRelativePath);
                        }
                    }
                }
                
                using var progressWrapper = new WrappedProgress(progress);
                var progress1 = progressWrapper.GetChildProgress(50);
                var progress2 = this.Sign.CurrentValue ? progressWrapper.GetChildProgress(30) : null;

                var tempFileList = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".list");
                temporaryFiles.Add(tempFileList);

                var tempManifestPath = Path.Combine(Path.GetTempPath(), "AppxManifest-" + Guid.NewGuid().ToString("N") + ".xml");
                temporaryFiles.Add(tempManifestPath);

                var srcManifest = fileListBuilder.GetManifestSourcePath();
                if (srcManifest == null || !File.Exists(srcManifest))
                {
                    throw new InvalidOperationException(Resources.Localization.Dialogs_Pack_NoManifestException);
                }

                // Copy manifest to a temporary file
                var injector = new MsixHeroBrandingInjector();
                await using (var manifestStream = File.OpenRead(fileListBuilder.GetManifestSourcePath()))
                {
                    var xml = await XDocument.LoadAsync(manifestStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
                    await injector.Inject(xml).ConfigureAwait(false);
                    await File.WriteAllTextAsync(tempManifestPath, xml.ToString(SaveOptions.None), cancellationToken);
                    fileListBuilder.AddManifest(tempManifestPath);
                }
                
                await File.WriteAllTextAsync(tempFileList, fileListBuilder.ToString(), cancellationToken).ConfigureAwait(false);

                var sdk = new MakeAppxWrapper();
                await sdk.Pack(MakeAppxPackOptions.CreateFromMapping(tempFileList, this.OutputPath.CurrentValue, this.Compress.CurrentValue, this.Validate.CurrentValue), progress1, cancellationToken).ConfigureAwait(false);
                
                if (this.Sign.CurrentValue)
                {
                    var manager = this._uacElevation.AsHighestAvailable<ISigningManager>();

                    string timeStampUrl;
                    switch (this.SelectedCertificate.TimeStampSelectionMode.CurrentValue)
                    {
                        case TimeStampSelectionMode.None:
                            timeStampUrl = null;
                            break;
                        case TimeStampSelectionMode.Auto:
                            timeStampUrl = "auto";
                            break;
                        case TimeStampSelectionMode.Url:
                            timeStampUrl = this.SelectedCertificate.TimeStamp.CurrentValue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (this.SelectedCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Personal:
                            await manager.SignPackageWithInstalled(this.OutputPath.CurrentValue, this.OverrideSubject.CurrentValue, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue?.Model, timeStampUrl, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                        case CertificateSource.Pfx:
                            await manager.SignPackageWithPfx(this.OutputPath.CurrentValue, this.OverrideSubject.CurrentValue, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, timeStampUrl, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                        case CertificateSource.DeviceGuard:
                            await manager.SignPackageWithDeviceGuardFromUi(this.OutputPath.CurrentValue, this.SelectedCertificate.DeviceGuard.CurrentValue, timeStampUrl, IncreaseVersionMethod.None, cancellationToken, progress2).ConfigureAwait(false);
                            break;
                    }
                }

                if (this.RemoveDirectory.CurrentValue)
                {
                    ExceptionGuard.Guard(() => Directory.Delete(this.InputPath.CurrentValue, true));
                }

                return true;
            }
            finally
            {
                foreach (var tempFile in temporaryFiles)
                {
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        private void ResetExecuted()
        {
            this.InputPath.Reset();
            this.OutputPath.Reset();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted()
        {
            Process.Start("explorer.exe", "/select," + this.OutputPath.CurrentValue);
        }

        private void SignOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.SelectedCertificate.IsValidated = this.Sign.CurrentValue;
        }

        private async void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.OutputPath.CurrentValue))
            {
                var newValue = (string)e.NewValue;
                this.OutputPath.CurrentValue = Path.Join(Path.GetDirectoryName(newValue), Path.GetFileName(newValue.TrimEnd('\\'))) + FileConstants.MsixExtension;
            }

            this.PrePackOptions = null;

            if (!string.IsNullOrEmpty(this.InputPath.CurrentValue))
            {
                this.IsLoadingPrePackOptions = true;
                this.OnPropertyChanged(nameof(IsLoadingPrePackOptions));
                AppxManifestCreatorAdviser adviser;
                try
                {
                    adviser = await this._manifestCreator.AnalyzeDirectory(new DirectoryInfo(this.InputPath.CurrentValue)).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    this.PrePackOptions = null;
                    this.OnPropertyChanged(nameof(PrePackOptions));
                    return;
                }
                finally
                {
                    this.IsLoadingPrePackOptions = false;
                    this.OnPropertyChanged(nameof(IsLoadingPrePackOptions));
                }

                if (adviser != null)
                {
                    this.PrePackOptions = new PrePackOptionsViewModel(adviser);
                }
            }

            this.OnPropertyChanged(nameof(PrePackOptions));
        }

        public PrePackOptionsViewModel PrePackOptions { get; private set; }

        public bool IsLoadingPrePackOptions { get; private set; }
    }
}

