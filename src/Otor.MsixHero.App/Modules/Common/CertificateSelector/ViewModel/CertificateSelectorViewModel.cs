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
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapplo.Log;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Appx.Signing.Testing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Common.CertificateSelector.ViewModel
{
    public class CertificateSelectorViewModel : ChangeableContainer
    {
        private static readonly LogSource Logger = new();
        private readonly IInteractionService _interactionService;
        private readonly IUacElevation _uacElevation;
        private readonly ITimeStampFeed _timeStampFeed;
        private ICommand _signOutDeviceGuard, _signInDeviceGuard;
        private readonly ISigningTestService _signTestService;
        private bool _allowNoSelection;

        public CertificateSelectorViewModel(
            ISigningTestService signTestService,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            SigningConfiguration signConfiguration,
            ITimeStampFeed timeStampFeed,
            bool allowNoSelection = false) : this(signTestService, interactionService, uacElevation, signConfiguration, signConfiguration.GetSelectedProfile(), timeStampFeed, allowNoSelection)
        {
        }

        public CertificateSelectorViewModel(
            ISigningTestService signTestService,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            SigningConfiguration signConfiguration,
            SigningProfile signProfile,
            ITimeStampFeed timeStampFeed,
            bool allowNoSelection = false)
        {
            this._signTestService = signTestService;
            this._allowNoSelection = allowNoSelection;
            this._interactionService = interactionService;
            this._uacElevation = uacElevation;
            this._timeStampFeed = timeStampFeed;

            var profile = signProfile ?? new SigningProfile();
            
            if (string.IsNullOrEmpty(profile.TimeStampServer))
            {
                this.TimeStampSelectionMode = new ChangeableProperty<TimeStampSelectionMode>();
            }
            else if (string.Equals("auto", profile.TimeStampServer, StringComparison.OrdinalIgnoreCase))
            {
                this.TimeStampSelectionMode = new ChangeableProperty<TimeStampSelectionMode>(ViewModel.TimeStampSelectionMode.Auto);
            }
            else
            {
                this.TimeStampSelectionMode = new ChangeableProperty<TimeStampSelectionMode>(ViewModel.TimeStampSelectionMode.Url);
            }

            this.Test = new DelegateCommand(this.OnTest, this.CanTest);

            this.TimeStamp = new ValidatedChangeableProperty<string>(
                Resources.Localization.CertificateSelector_TimeStamp,
                profile.TimeStampServer == "auto" ? null : profile.TimeStampServer,
                this.ValidateTimestamp);

            this.TimeStampSelectionMode.Changed += (_, _) =>
            {
                this.TimeStamp.Validate();
            };

            var newStore = profile.Source;
            if (newStore == CertificateSource.Unknown && !allowNoSelection)
            {
                newStore = CertificateSource.Personal;
            }

            this.Store = new ChangeableProperty<CertificateSource>(newStore);
            this.Store.ValueChanged += this.StoreOnValueChanged;
            this.PfxPath = new ChangeableFileProperty(Resources.Localization.CertificateSelector_PfxPath, interactionService, profile.PfxPath?.Resolved, this.ValidatePfxPath)
            {
                Filter = new DialogFilterBuilder().WithCertificates(DialogFilterBuilderPackagesExtensions.CertificateTypes.Pfx).WithAll().Build()
            };

            SecureString initialSecurePassword = null;

            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                var initialPassword = profile.EncodedPassword;

                if (!string.IsNullOrEmpty(initialPassword))
                {
                    var crypto = new Crypto();
                    try
                    {
                        initialSecurePassword = crypto.Unprotect(initialPassword);
                    }
                    catch (Exception)
                    {
                        Logger.Warn().WriteLine(Resources.Localization.CertificateSelector_Errors_PasswordEncryption);
                    }
                }
            }
            
            this.Password = new ChangeableProperty<SecureString>(initialSecurePassword);

            if (signProfile?.DeviceGuard?.EncodedAccessToken != null)
            {
                this.DeviceGuard = new ValidatedChangeableProperty<DeviceGuardConfiguration>(Resources.Localization.CertificateSelector_DeviceGuard, signProfile.DeviceGuard, false, this.ValidateDeviceGuard);
            }
            else
            {
                this.DeviceGuard = new ValidatedChangeableProperty<DeviceGuardConfiguration>(Resources.Localization.CertificateSelector_DeviceGuard, null, false, this.ValidateDeviceGuard);
            }

            this.SelectedPersonalCertificate = new ValidatedChangeableProperty<CertificateViewModel>(Resources.Localization.CertificateSelector_SelectedCertificate, false, this.ValidateSelectedCertificate);
            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates(profile.Thumbprint, !signConfiguration.ShowAllCertificates));
            this.ShowAllCertificates = new ChangeableProperty<bool>(signConfiguration.ShowAllCertificates);
            this.AddChildren(this.SelectedPersonalCertificate, this.PfxPath, this.TimeStamp, this.TimeStampSelectionMode, this.Password, this.DeviceGuard, this.Store, this.ShowAllCertificates);
            
            this.ShowAllCertificates.ValueChanged += async (_, args) =>
            {
                await this.PersonalCertificates.Load(this.LoadPersonalCertificates(this.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint, !(bool)args.NewValue)).ConfigureAwait(false);
                this.OnPropertyChanged(nameof(this.SelectedPersonalCertificate));
            };
        }

        private void StoreOnValueChanged(object sender, EventArgs e)
        {
            this.PfxPath.Validate();
            this.DeviceGuard.Validate();
            this.SelectedPersonalCertificate.Validate();
        }

        private async void OnTest()
        {
            Task<SignTestResult> task;

            string timeStampServer = null;

            // if random or none is selected, we should avoid testing the timestamp server.
            if (this.TimeStampSelectionMode.CurrentValue == ViewModel.TimeStampSelectionMode.Url)
            {
                timeStampServer = this.TimeStamp.CurrentValue;
            }

            switch (this.Store.CurrentValue)
            {
                case CertificateSource.Personal:
                    task = this._signTestService.VerifyInstalled(this.SelectedPersonalCertificate.CurrentValue?.Thumbprint, timeStampServer);
                    break;
                case CertificateSource.Pfx:
                    task = this._signTestService.VerifyPfx(this.PfxPath.CurrentValue, this.Password.CurrentValue, timeStampServer);
                    break;
                case CertificateSource.DeviceGuard:
                    task = this._signTestService.VerifyDeviceGuardSettings(this.DeviceGuard.CurrentValue, timeStampServer);
                    break;
                default:
                    return;
            }

            this.Progress.MonitorProgress(task);

            var result = await task.ConfigureAwait(false);

            switch (result.Type)
            {
                case SignTestResultType.Ok:
                    this._interactionService.ShowInfo(Resources.Localization.Signing_Test_OK);
                    break;
                case SignTestResultType.Warn:
                    this._interactionService.ShowInfo(result.Message);
                    break;
                default:
                    this._interactionService.ShowError(result.Message);
                    break;
            }
        }

        private bool CanTest()
        {
            return this.Store.CurrentValue != CertificateSource.Unknown;
        }

        public ICommand Test { get; }

        public string ValidatePfxPath(string value)
        {
            if (this.Store.CurrentValue != CertificateSource.Pfx)
            {
                return null;
            }

            return ChangeableFileProperty.ValidatePathAndPresence(value);
        }

        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public ValidatedChangeableProperty<CertificateViewModel> SelectedPersonalCertificate { get; }

        public ChangeableProperty<SecureString> Password { get; }

        public ValidatedChangeableProperty<DeviceGuardConfiguration> DeviceGuard { get; }

        public ChangeableProperty<CertificateSource> Store { get; }
        
        public ProgressProperty IsTesting { get; } = new ProgressProperty();

        public bool AllowNoSelection
        {
            get => this._allowNoSelection;
            set
            {
                if (!this.SetField(ref this._allowNoSelection, value))
                {
                    return;
                }

                if (this.Store.CurrentValue == CertificateSource.Unknown)
                {
                    this.Store.CurrentValue = CertificateSource.Pfx;
                }
            }
        }

        public ICommand SignInDeviceGuard
        {
            get
            {
                return this._signInDeviceGuard ??= new DelegateCommand(this.ExecuteSignInDeviceGuard);
            }
        }

        public ICommand SignOutDeviceGuard
        {
            get
            {
                return this._signOutDeviceGuard ??= new DelegateCommand(this.ExecuteSignOutDeviceGuard);
            }
        }

        private void ExecuteSignOutDeviceGuard()
        {
            this.DeviceGuard.CurrentValue = null;
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        private async void ExecuteSignInDeviceGuard()
        {
            var tokenCreator = new DgssTokenCreator();
            
            try
            {
                this.Progress.Progress = 0;
                this.Progress.Message = Resources.Localization.CertificateSelector_DeviceGuard_Signing;

                using var cancellation = new CancellationTokenSource();
                IProgress<ProgressData> progress = new Progress<ProgressData>();
                var task = tokenCreator.SignIn(true, cancellation.Token, progress);
                this.Progress.MonitorProgress(task, cancellation, progress);
                
                var result = await task.ConfigureAwait(true);
                
                var crypto = new Crypto();

                if (this.DeviceGuard.CurrentValue == null)
                {
                    this.DeviceGuard.CurrentValue = new DeviceGuardConfiguration
                    {
                        EncodedAccessToken = crypto.Protect(result.AccessToken),
                        EncodedRefreshToken = crypto.Protect(result.RefreshToken),
                        Subject = result.Subject
                    };
                }
                else
                {
                    this.DeviceGuard.CurrentValue = new DeviceGuardConfiguration
                    {
                        EncodedAccessToken = crypto.Protect(result.AccessToken),
                        EncodedRefreshToken = crypto.Protect(result.RefreshToken),
                        Subject = result.Subject
                    };
                }
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(e.Message, e, InteractionResult.OK);
            }
        }

        private async Task<ObservableCollection<CertificateViewModel>> LoadPersonalCertificates(string thumbprint = null, bool onlyValid = true, CancellationToken cancellationToken = default)
        {
            var manager = this._uacElevation.AsCurrentUser<ISigningManager>();
            cancellationToken.ThrowIfCancellationRequested();

            var needsCommit = this.SelectedPersonalCertificate.CurrentValue == null;
            var certs = await manager.GetCertificatesFromStore(CertificateStoreType.MachineUser, onlyValid, cancellationToken).ConfigureAwait(false);
            var result = new ObservableCollection<CertificateViewModel>(certs.Select(c => new CertificateViewModel(c)));
            this.SelectedPersonalCertificate.CurrentValue = result.FirstOrDefault(c => thumbprint == null || c.Model.Thumbprint == thumbprint) ?? result.FirstOrDefault();

            if (needsCommit)
            {
                this.SelectedPersonalCertificate.Commit();
            }

            return result;
        }

        public ChangeableFileProperty PfxPath { get; }

        public ValidatedChangeableProperty<string> TimeStamp { get; }
        
        public ChangeableProperty<bool> ShowAllCertificates { get; }

        private string ValidateSelectedCertificate(CertificateViewModel arg)
        {
            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                return null;
            }

            return arg == null ? Resources.Localization.CertificateSelector_Validation_CertificateMissing : null;
        }

        private string ValidateDeviceGuard(DeviceGuardConfiguration arg)
        {
            if (this.Store.CurrentValue != CertificateSource.DeviceGuard)
            {
                return null;
            }

            return arg == null ? Resources.Localization.CertificateSelector_Validation_DeviceGuardNotConfigured : null;
        }

        public ChangeableProperty<TimeStampSelectionMode> TimeStampSelectionMode { get; }

        private string ValidateTimestamp(string value)
        {
            if (this.TimeStampSelectionMode.CurrentValue != ViewModel.TimeStampSelectionMode.Url)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                return Resources.Localization.CertificateSelector_Validation_TimeStampMissing;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return string.Format(Resources.Localization.CertificateSelector_Validation_InvalidUrl_Format, value);
            }

            if (string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||  string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Resources.Localization.CertificateSelector_Validation_UrlProtocolMissing;
        }

        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        public AsyncProperty<IList<string>> TimeStampServers { get; } = new AsyncProperty<IList<string>>();

        public async Task<IList<string>> GenerateTimeStampServers()
        {
            try
            {
                var newList = new List<string>();
                var entries = await this._timeStampFeed.GetTimeStampServers().ConfigureAwait(false);
                foreach (var item in entries?.Servers ?? Enumerable.Empty<TimeStampServerEntry>())
                {
                    newList.Add(item.Url);
                }

                var currentTimeStamp = this.TimeStamp.CurrentValue;
                if (!newList.Contains(currentTimeStamp))
                {
                    newList.Add(currentTimeStamp);
                }

                return newList;
            }
            catch (OperationCanceledException)
            {
                // cancelled by the user
                return null;
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(Resources.Localization.CertificateSelector_Validation_RemoteUrlsCouldNotBeFetched, e);
                return null;
            }
        }
    }

    public enum TimeStampSelectionMode
    {
        None,
        Auto,
        Url
    }
}

