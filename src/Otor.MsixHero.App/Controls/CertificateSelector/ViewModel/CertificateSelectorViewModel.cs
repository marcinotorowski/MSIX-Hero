// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Controls.CertificateSelector.ViewModel
{
    public class CertificateSelectorViewModel : ChangeableContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertificateSelectorViewModel));

        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private ICommand signOutDeviceGuard, signInDeviceGuard;
        private bool allowNoSelection;

        public CertificateSelectorViewModel(
            IInteractionService interactionService,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory,
            SigningConfiguration configuration,
            bool allowNoSelection = false)
        {
            this.allowNoSelection = allowNoSelection;
            this.interactionService = interactionService;
            this.signingManagerFactory = signingManagerFactory;
            var signConfig = configuration ?? new SigningConfiguration();

            this.TimeStamp = new ValidatedChangeableProperty<string>(
                "Time stamp URL",
                signConfig.TimeStampServer ?? "http://timestamp.globalsign.com/scripts/timstamp.dll",
                this.ValidateTimestamp);

            var newStore = signConfig.Source;
            if (newStore == CertificateSource.Unknown && !allowNoSelection)
            {
                newStore = CertificateSource.Personal;
            }

            this.Store = new ChangeableProperty<CertificateSource>(newStore);
            this.Store.ValueChanged += StoreOnValueChanged;
            this.PfxPath = new ChangeableFileProperty("Path to PFX file", interactionService, signConfig.PfxPath?.Resolved)
            {
                IsValidated = false,
                Filter = "PFX files|*.pfx", 
                Validators = new []
                {
                    ChangeableFileProperty.ValidatePathAndPresence
                }
            };

            SecureString initialSecurePassword = null;

            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                var initialPassword = signConfig.EncodedPassword;

                if (!string.IsNullOrEmpty(initialPassword))
                {
                    var crypto = new Crypto();
                    try
                    {
                        initialSecurePassword = crypto.Unprotect(initialPassword);
                    }
                    catch (Exception)
                    {
                        Logger.Warn("The encrypted password from settings could not be decrypted.");
                    }
                }
            }
            
            this.Password = new ChangeableProperty<SecureString>(initialSecurePassword);

            if (configuration?.DeviceGuard?.EncodedAccessToken != null)
            {
                this.DeviceGuard = new ValidatedChangeableProperty<DeviceGuardConfiguration>("Device Guard", configuration.DeviceGuard, false, this.ValidateDeviceGuard);
            }
            else
            {
                this.DeviceGuard = new ValidatedChangeableProperty<DeviceGuardConfiguration>("Device Guard", null, false, this.ValidateDeviceGuard);
            }

            this.SelectedPersonalCertificate = new ValidatedChangeableProperty<CertificateViewModel>("Selected certificate", false, this.ValidateSelectedCertificate);
            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates(signConfig.Thumbprint, !signConfig.ShowAllCertificates));
            this.ShowAllCertificates = new ChangeableProperty<bool>(signConfig.ShowAllCertificates);

            switch (this.Store.CurrentValue)
            {
                case CertificateSource.Pfx:
                    this.PfxPath.IsValidated = true;
                    break;
                case CertificateSource.DeviceGuard:
                    this.DeviceGuard.IsValidated = true;
                    break;
                case CertificateSource.Personal:
                    this.SelectedPersonalCertificate.IsValidated = true;
                    break;
            }

            this.AddChildren(this.SelectedPersonalCertificate, this.PfxPath, this.TimeStamp, this.Password, this.DeviceGuard, this.Store, this.ShowAllCertificates);
            
            this.ShowAllCertificates.ValueChanged += async (sender, args) =>
            {
                await this.PersonalCertificates.Load(this.LoadPersonalCertificates(this.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint, !(bool)args.NewValue)).ConfigureAwait(false);
                this.OnPropertyChanged(nameof(this.SelectedPersonalCertificate));
            };
        }
        
        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public ValidatedChangeableProperty<CertificateViewModel> SelectedPersonalCertificate { get; }

        public ChangeableProperty<SecureString> Password { get; }

        public ValidatedChangeableProperty<DeviceGuardConfiguration> DeviceGuard { get; }

        public ChangeableProperty<CertificateSource> Store { get; }

        public bool AllowNoSelection
        {
            get => this.allowNoSelection;
            set
            {
                if (!this.SetField(ref this.allowNoSelection, value))
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
                return this.signInDeviceGuard ??= new DelegateCommand(this.ExecuteSignInDeviceGuard);
            }
        }

        public ICommand SignOutDeviceGuard
        {
            get
            {
                return this.signOutDeviceGuard ??= new DelegateCommand(this.ExecuteSignOutDeviceGuard);
            }
        }

        private void ExecuteSignOutDeviceGuard()
        {
            this.DeviceGuard.CurrentValue = null;
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        private async void ExecuteSignInDeviceGuard()
        {
            var dgh = new DgssTokenCreator();
            
            try
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    IProgress<ProgressData> progress = new Progress<ProgressData>();
                    var task = dgh.SignIn(cancellation.Token, progress);
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
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e, InteractionResult.OK);
            }
        }

        private async Task<ObservableCollection<CertificateViewModel>> LoadPersonalCertificates(string thumbprint = null, bool onlyValid = true, CancellationToken cancellationToken = default)
        {
            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, CancellationToken.None).ConfigureAwait(false);
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

        public ChangeableProperty<string> TimeStamp { get; }
        
        public ChangeableProperty<bool> ShowAllCertificates { get; }

        private string ValidateSelectedCertificate(CertificateViewModel arg)
        {
            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                return null;
            }

            return arg == null ? "The certificate is required." : null;
        }

        private string ValidateDeviceGuard(DeviceGuardConfiguration arg)
        {
            if (this.Store.CurrentValue != CertificateSource.DeviceGuard)
            {
                return null;
            }

            return arg == null ? "The configuration for the device guard is required." : null;
        }

        private string ValidateTimestamp(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Timestamp server URL is required.";
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return $"The value '{value}' is not a valid URL.";
            }

            if (string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||  string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return "The URL must have a protocol.";
        }

        private void StoreOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            switch ((CertificateSource) e.NewValue)
            {
                case CertificateSource.Unknown:
                    this.PfxPath.IsValidated = false;
                    this.SelectedPersonalCertificate.IsValidated = false;
                    this.DeviceGuard.IsValidated = false;
                    break;
                case CertificateSource.Personal:
                    this.PfxPath.IsValidated = false;
                    this.SelectedPersonalCertificate.IsValidated = true;
                    this.DeviceGuard.IsValidated = false;
                    break;
                case CertificateSource.Pfx:
                    this.PfxPath.IsValidated = true;
                    this.SelectedPersonalCertificate.IsValidated = false;
                    this.DeviceGuard.IsValidated = false;
                    break;
                case CertificateSource.DeviceGuard:
                    this.PfxPath.IsValidated = false;
                    this.SelectedPersonalCertificate.IsValidated = false;
                    this.DeviceGuard.IsValidated = true;
                    break;
            }
        }
    }
}

