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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummarySignatureViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private readonly IInteractionService _interactionService;
        private readonly IUacElevation _uacElevation;
        private bool _isTrusting;
        private DelegateCommand _trustMe;
        private DelegateCommand _showPropertiesCommand;
        private string _certificateFile;
        private string _packagePath;

        private bool _isLoading;

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public SummarySignatureViewModel(IInteractionService interactionService, IUacElevation uacElevation)
        {
            this._interactionService = interactionService;
            this._uacElevation = uacElevation;
        }

        public bool IsTrusting
        {
            get => this._isTrusting;
            set => this.SetField(ref this._isTrusting, value);
        }
        
        public ICommand TrustMeCommand
        {
            get
            {
                // ReSharper disable once AsyncVoidLambda
                return this._trustMe ??= new DelegateCommand(async () =>
                    {
                        if (this._certificateFile == null)
                        {
                            return;
                        }

                        if (this._interactionService.Confirm(Resources.Localization.PackageExpert_Trust_Prompt, type: InteractionType.Question, buttons: InteractionButton.YesNo) != InteractionResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            this.IsTrusting = true;
                            
                            await this._uacElevation.AsAdministrator<ISigningManager>().Trust(this._certificateFile).ConfigureAwait(false);
                            await this.TrustStatus.Load(this.LoadSignature(this._packagePath, CancellationToken.None)).ConfigureAwait(false);
                        }
                        finally
                        {
                            this.IsTrusting = false;
                        }
                    });
            }
        }

        public ICommand ShowPropertiesCommand
        {
            get
            {
                return this._showPropertiesCommand ??= new DelegateCommand(() =>
                {
                    if (this._certificateFile == null)
                    {
                        return;
                    }

                    WindowsExplorerCertificateHelper.ShowFileSecurityProperties(this._certificateFile, IntPtr.Zero);
                });
            }
        }

        public AsyncProperty<TrustStatus> TrustStatus { get; } = new AsyncProperty<TrustStatus>(isLoading: true);

        public Task LoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            var _ = this.TrustStatus.Load(this.LoadSignature(filePath, cancellationToken));
            return Task.CompletedTask;
        }

        private async Task<TrustStatus> LoadSignature(string packagePath, CancellationToken cancellationToken)
        {
            try
            {
                this.IsLoading = true;
                this._packagePath = packagePath;
                using var source = FileReaderFactory.CreateFileReader(this._packagePath);

                this._certificateFile = null;

                if (source is ZipArchiveFileReaderAdapter zipFileReader)
                {
                    this._certificateFile = zipFileReader.PackagePath;
                    var signTask = this._uacElevation.AsHighestAvailable<ISigningManager>().IsTrusted(zipFileReader.PackagePath, cancellationToken);
                    await this.TrustStatus.Load(signTask);
                    return await signTask.ConfigureAwait(false);
                }

                if (source is IAppxDiskFileReader fileReader)
                {
                    var file = new FileInfo(Path.Combine(fileReader.RootDirectory, "AppxSignature.p7x"));
                    
                    if (file.Exists)
                    {
                        this._certificateFile = file.FullName;
                        var signTask = this._uacElevation.AsHighestAvailable<ISigningManager>().IsTrusted(file.FullName, cancellationToken);
                        await this.TrustStatus.Load(signTask);
                        return await signTask.ConfigureAwait(false);
                    }
                }

                // not signed
                await this.TrustStatus.Load(Task.FromResult(default(TrustStatus))).ConfigureAwait(false);
                return default;
            }
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}
