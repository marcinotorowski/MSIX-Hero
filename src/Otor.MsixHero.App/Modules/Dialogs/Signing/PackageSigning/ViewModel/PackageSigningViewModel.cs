﻿// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Modules.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Appx.Signing.Testing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : ChangeableAutomatedDialogViewModel<SignVerb>, IDialogAware
    {
        private readonly IUacElevation _uacElevation;
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private ICommand _openSuccessLink, _reset;

        public PackageSigningViewModel(
            ISigningTestService signTestService,
            IUacElevation uacElevation, 
            IInteractionService interactionService, 
            IConfigurationService configurationService,
            ITimeStampFeed timeStampFeed) : base(Resources.Localization.Dialogs_PackageSigning_Title, interactionService)
        {
            this._uacElevation = uacElevation;
            this._interactionService = interactionService;
            this._configurationService = configurationService;

            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.IncreaseVersion = new ChangeableProperty<IncreaseVersionMethod>();
            this.CertificateSelector = new CertificateSelectorViewModel(
                signTestService,
                interactionService, 
                uacElevation, 
                configurationService?.GetCurrentConfiguration()?.Signing,
                timeStampFeed);
            this.OverrideSubject = new ChangeableProperty<bool>(true);

            this.TabPackages = new ChangeableContainer(this.Files);
            this.TabAdjustments = new ChangeableContainer(this.IncreaseVersion);
            this.TabCertificate = new ChangeableContainer(this.CertificateSelector);

            this.AddChildren(this.TabPackages, this.TabCertificate, this.TabAdjustments, this.OverrideSubject);
            this.Files.CollectionChanged += (_, _) =>
            {
                this.OnPropertyChanged(nameof(IsOnePackage));
            };

            this.RegisterForCommandLineGeneration(
                this.TabCertificate,
                this.TabPackages,
                this.TabAdjustments, 
                this.OverrideSubject);
        }
        
        protected override void UpdateVerbData()
        {
            this.Verb.FilePath = this.Files;
            if (!this.Verb.FilePath.Any())
            {
                this.Verb.FilePath = new[] { Resources.Localization.Dialogs_PackageSigning_Placeholder_Msix };
            }

            this.Verb.IncreaseVersion = this.IncreaseVersion.CurrentValue;
            this.Verb.NoPublisherUpdate = !this.OverrideSubject.CurrentValue;

            var signConfig = this._configurationService.GetCurrentConfiguration().Signing;

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.DeviceGuard)
            {
                if (signConfig?.Source == CertificateSource.DeviceGuard && 
                    signConfig.DeviceGuard.Subject == this.CertificateSelector.DeviceGuard.CurrentValue.Subject &&
                    signConfig.DeviceGuard.EncodedAccessToken == this.CertificateSelector.DeviceGuard.CurrentValue.EncodedAccessToken &&
                    signConfig.DeviceGuard.EncodedRefreshToken == this.CertificateSelector.DeviceGuard.CurrentValue.EncodedRefreshToken)
                {
                    // do nothing, we have defaults so ideally just no additional command line
                    this.Verb.DeviceGuardInteractive = false;
                    this.Verb.DeviceGuardFile = null;
                    this.Verb.DeviceGuardSubject = null;
                }
                else
                {
                    this.Verb.DeviceGuardInteractive = true;
                    this.Verb.DeviceGuardFile = null;
                    this.Verb.DeviceGuardSubject = this.CertificateSelector.DeviceGuard.CurrentValue?.Subject;
                }
            }
            else
            {
                this.Verb.DeviceGuardInteractive = false;
                this.Verb.DeviceGuardFile = null;
                this.Verb.DeviceGuardSubject = null;
            }

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Pfx)
            {
                this.Verb.PfxFilePath = this.CertificateSelector.PfxPath.CurrentValue;
                this.Verb.PfxPassword = this.CertificateSelector.Password.CurrentValue?.Length > 0 ? Resources.Localization.Dialogs_PackageSigning_Placeholder_Pwd : null;

                if (string.IsNullOrEmpty(this.Verb.PfxFilePath))
                {
                    this.Verb.PfxFilePath = Resources.Localization.Dialogs_PackageSigning_Placeholder_Pfx;
                }
            }
            else
            {
                this.Verb.PfxFilePath = null;
                this.Verb.PfxPassword = null;
            }

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Personal)
            {
                this.Verb.ThumbPrint = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint;
                var store = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.StoreType;

                switch (store)
                {
                    case CertificateStoreType.Machine:
                    case CertificateStoreType.MachineUser:
                        this.Verb.UseMachineStore = true;
                        break;
                    default:
                        this.Verb.UseMachineStore = false;
                        break;
                }

                if (string.IsNullOrEmpty(this.Verb.ThumbPrint))
                {
                    this.Verb.ThumbPrint = Resources.Localization.Dialogs_PackageSigning_Placeholder_Thumbprint;
                }
            }
            else
            {
                this.Verb.ThumbPrint = null;
                this.Verb.UseMachineStore = false;
            }

            this.Verb.NoPublisherUpdate = !this.OverrideSubject.CurrentValue;

            switch (this.CertificateSelector.TimeStampSelectionMode.CurrentValue)
            {
                case TimeStampSelectionMode.None:
                    this.Verb.TimeStampUrl = null;
                    break;
                case TimeStampSelectionMode.Auto:
                    this.Verb.TimeStampUrl = "auto";
                    break;
                case TimeStampSelectionMode.Url:
                    this.Verb.TimeStampUrl = this.CertificateSelector.TimeStamp.CurrentValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ChangeableContainer TabPackages { get; }

        public ChangeableContainer TabAdjustments { get; }

        public ChangeableContainer TabCertificate { get; }

        public CertificateSelectorViewModel CertificateSelector { get; }

        public ChangeableProperty<IncreaseVersionMethod> IncreaseVersion { get; }

        public ChangeableProperty<bool> OverrideSubject { get; }

        public List<string> SelectedPackages { get; } = new List<string>();

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Path", out var file))
            {
                this.Files.Add(file);
            }
            else
            {
                var filter = new DialogFilterBuilder().WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.Msix).WithAll();
                var interactionResult = this._interactionService.SelectFiles(FileDialogSettings.FromFilterString(filter), out var selection);
                if (!interactionResult || !selection.Any())
                {
                    return;
                }

                foreach (var selected in selection)
                {
                    if (this.Files.Contains(selected, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    this.Files.Add(selected);
                }
            }

            this.Files.Commit();
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var manager = this._uacElevation.AsCurrentUser<ISigningManager>();

            cancellationToken.ThrowIfCancellationRequested();

            using var progressAll = new WrappedProgress(progress);
            // ReSharper disable once AccessToDisposedClosure
            var progressForFiles = this.Files.ToDictionary(p => p, _ => progressAll.GetChildProgress());
                
            foreach (var file in this.Files)
            {
                var currentProgress = progressForFiles[file];
                    
                cancellationToken.ThrowIfCancellationRequested();

                string timeStampUrl;
                switch (this.CertificateSelector.TimeStampSelectionMode.CurrentValue)
                {
                    case TimeStampSelectionMode.None:
                        timeStampUrl = null;
                        break;
                    case TimeStampSelectionMode.Auto:
                        timeStampUrl = "auto";
                        break;
                    case TimeStampSelectionMode.Url:
                        timeStampUrl = this.CertificateSelector.TimeStamp.CurrentValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (this.CertificateSelector.Store.CurrentValue)
                {
                    case CertificateSource.Pfx:
                        await manager.SignPackageWithPfx(file, this.OverrideSubject.CurrentValue, this.CertificateSelector.PfxPath.CurrentValue, this.CertificateSelector.Password.CurrentValue, timeStampUrl, this.IncreaseVersion.CurrentValue, cancellationToken, currentProgress).ConfigureAwait(false);
                        break;
                    case CertificateSource.Personal:
                        await manager.SignPackageWithInstalled(file, this.OverrideSubject.CurrentValue, this.CertificateSelector.SelectedPersonalCertificate.CurrentValue.Model, timeStampUrl, this.IncreaseVersion.CurrentValue, cancellationToken, currentProgress).ConfigureAwait(false);
                        break;
                    case CertificateSource.DeviceGuard:
                        await manager.SignPackageWithDeviceGuardFromUi(file, this.CertificateSelector.DeviceGuard.CurrentValue, timeStampUrl, this.IncreaseVersion.CurrentValue, cancellationToken, currentProgress).ConfigureAwait(false);
                        break;
                }
            }

            return true;
        }

        public ValidatedChangeableCollection<string> Files { get; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this._openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted, this.CanOpenSuccessLinkExecute); }
        }

        public bool IsOnePackage
        {
            get => this.Files.Count == 1;
        }

        public ICommand ResetCommand
        {
            get { return this._reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public async Task<int> ImportFolder()
        {
            if (!this._interactionService.SelectFolder(out var folder))
            {
                return 0;
            }

            var hasMsixFiles = Directory.EnumerateFiles(folder, "*" + FileExtensions.Msix).Any();
            var hasSubfolders = Directory.EnumerateDirectories(folder).Any();

            var recurse = !hasMsixFiles;

            if (!recurse && hasSubfolders)
            {
                IReadOnlyCollection<string> buttons = new List<string>
                { 
                    Resources.Localization.Dialogs_PackageSigning_ImportFolder_Title,
                    string.Format(Resources.Localization.Dialogs_PackageSigning_ImportFolder_Folder_Format, Path.GetFileName(folder))
                };

                var userChoice = this._interactionService.ShowMessage(Resources.Localization.Dialogs_PackageSigning_ImportFolder_Multiple, buttons, systemButtons: InteractionResult.Cancel);
                if (userChoice < 0 || userChoice >= buttons.Count)
                {
                    return 0;
                }

                recurse = userChoice == 1;
            }
            
            var files = await Task.Run(() => Directory.EnumerateFiles(folder, "*" + FileExtensions.Msix, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList()).ConfigureAwait(true);
            var cnt = this.Files.Count;
            this.Files.AddRange(files.Except(this.Files, StringComparer.OrdinalIgnoreCase));

            return this.Files.Count - cnt;
        }

        private string ValidateFiles(IEnumerable<string> files)
        {
            if (!files.Any())
            {
                return Resources.Localization.Dialogs_PackageSigning_Validation_FileNotSelected;
            }

            return null;
        }

        private void ResetExecuted()
        {
            this.Files.Clear();
            this.Commit();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted()
        {
            if (!this.IsOnePackage)
            {
                return;
            }

            Process.Start("explorer.exe", "/select," + this.Files[0]);
        }

        private bool CanOpenSuccessLinkExecute()
        {
            return this.IsOnePackage;
        }
    }
}

