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
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
{
    public class TrustViewModel : NotifyPropertyChanged
    {
        private readonly string _packagePath;
        private readonly IInteractionService _interactionService;
        private readonly IUacElevation _uacElevation;
        private bool _isTrusting;
        private DelegateCommand _trustMe;
        private DelegateCommand _showPropertiesCommand;
        private string _certificateFile;

        public TrustViewModel(
            string packagePath,
            IInteractionService interactionService,
            IUacElevation uacElevation)
        {
            this._packagePath = packagePath;
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
                return this._trustMe ??= new DelegateCommand(async () =>
                    {
                        if (this._interactionService.Confirm("Are you sure you want to add this publisher to the list of trusted publishers (machine-wide)?", type: InteractionType.Question, buttons: InteractionButton.YesNo) != InteractionResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            this.IsTrusting = true;
                            
                            await this._uacElevation.AsAdministrator<ISigningManager>().Trust(this._certificateFile).ConfigureAwait(false);
                            await this.TrustStatus.Load(this.LoadSignature(CancellationToken.None)).ConfigureAwait(false);
                        }
                        finally
                        {
                            this.IsTrusting = false;
                        }
                    },
                    () => this._certificateFile != null);
            }
        }

        public ICommand ShowPropertiesCommand
        {
            get
            {
                return this._showPropertiesCommand ??= new DelegateCommand(() =>
                {
                    WindowsExplorerCertificateHelper.ShowFileSecurityProperties(this._certificateFile, IntPtr.Zero);
                }, () => this._certificateFile != null);
            }
        }

        public AsyncProperty<TrustStatus> TrustStatus { get; } = new AsyncProperty<TrustStatus>();

        public async Task<TrustStatus> LoadSignature(CancellationToken cancellationToken)
        {
            using var source = FileReaderFactory.CreateFileReader(this._packagePath);
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
                this._certificateFile = file.FullName;
                if (file.Exists)
                {
                    var signTask = this._uacElevation.AsHighestAvailable<ISigningManager>().IsTrusted(file.FullName, cancellationToken);
                    await this.TrustStatus.Load(signTask);
                    return await signTask.ConfigureAwait(false);
                }
            }

            return null;
        }

    }
}
