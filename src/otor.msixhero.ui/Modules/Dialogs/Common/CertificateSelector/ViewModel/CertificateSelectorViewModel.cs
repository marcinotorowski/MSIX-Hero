using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Helpers;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.ViewModel
{
    public class CertificateSelectorViewModel : ChangeableContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertificateSelectorViewModel));

        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;

        public CertificateSelectorViewModel(
            IInteractionService interactionService,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory,
            SigningConfiguration configuration,
            bool showPassword)
        {
            this.signingManagerFactory = signingManagerFactory;
            var signConfig = configuration ?? new SigningConfiguration();

            this.TimeStamp = new ValidatedChangeableProperty<string>(
                "Time stamp URL",
                signConfig.TimeStampServer ?? "http://timestamp.globalsign.com/scripts/timstamp.dll",
                this.ValidateTimestamp);

            this.ClientId = new ValidatedChangeableProperty<string>("ClientId", signConfig.ClientId, false, this.ValidateGuid);

            this.Store = new ChangeableProperty<CertificateSource>(signConfig.Source);
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
            SecureString initialSecureSecret = null;

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

            if (this.Store.CurrentValue == CertificateSource.DeviceGuard)
            {
                var initialSecret = signConfig.EncodedSecret;
                if (!string.IsNullOrEmpty(initialSecret))
                {
                    var crypto = new Crypto();
                    try
                    {
                        initialSecureSecret = crypto.Unprotect(initialSecret);
                    }
                    catch (Exception)
                    {
                        Logger.Warn("The encrypted secret from settings could not be decrypted.");
                    }
                }
            }   

            this.Password = new ChangeableProperty<SecureString>(initialSecurePassword);
            this.Secret = new ValidatedChangeableProperty<SecureString>("Client secret", initialSecureSecret, false);

            this.SelectedPersonalCertificate = new ValidatedChangeableProperty<CertificateViewModel>("Selected certificate", false, this.ValidateSelectedCertificate);
            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates(signConfig.Thumbprint, !signConfig.ShowAllCertificates));
            this.ShowAllCertificates = new ChangeableProperty<bool>(signConfig.ShowAllCertificates);

            this.AddChildren(this.SelectedPersonalCertificate, this.PfxPath, this.TimeStamp, this.Password, this.ClientId, this.Secret, this.Store, this.ShowAllCertificates);
            this.ShowPassword = showPassword;

            this.ShowAllCertificates.ValueChanged += async (sender, args) =>
            {
                await this.PersonalCertificates.Load(this.LoadPersonalCertificates(this.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint, !(bool)args.NewValue)).ConfigureAwait(false);
                this.OnPropertyChanged(nameof(this.SelectedPersonalCertificate));
            };

            switch (this.Store.CurrentValue)
            {
                case CertificateSource.Pfx:
                    this.PfxPath.IsValidated = true;
                    break;
                case CertificateSource.DeviceGuard:
                    this.Secret.IsValidated = true;
                    this.ClientId.IsValidated = true;
                    break;
                case CertificateSource.Personal:
                    this.SelectedPersonalCertificate.IsValidated = true;
                    break;
            }
        }

        public bool ShowPassword { get; }

        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public ValidatedChangeableProperty<CertificateViewModel> SelectedPersonalCertificate { get; }

        public ChangeableProperty<SecureString> Password { get; }

        public ValidatedChangeableProperty<SecureString> Secret { get; }

        public ChangeableProperty<CertificateSource> Store { get; }

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

        public ValidatedChangeableProperty<string> ClientId { get; }

        public ChangeableProperty<bool> ShowAllCertificates { get; }

        private string ValidateSelectedCertificate(CertificateViewModel arg)
        {
            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                return null;
            }

            return arg == null ? "The certificate is required." : null;
        }

        private string ValidateGuid(string value)
        {
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out _))
            {
                return "The value must be a GUID.";
            }

            return null;
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
                case CertificateSource.Personal:
                    this.PfxPath.IsValidated = false;
                    this.SelectedPersonalCertificate.IsValidated = true;
                    this.Secret.IsValidated = false;
                    this.ClientId.IsValidated = false;
                    break;
                case CertificateSource.Pfx:
                    this.PfxPath.IsValidated = true;
                    this.SelectedPersonalCertificate.IsValidated = false;
                    this.Secret.IsValidated = false;
                    this.ClientId.IsValidated = false;
                    break;
                case CertificateSource.DeviceGuard:
                    this.PfxPath.IsValidated = false;
                    this.SelectedPersonalCertificate.IsValidated = false;
                    this.Secret.IsValidated = true;
                    this.ClientId.IsValidated = true;
                    break;
            }
        }
    }
}

