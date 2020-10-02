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
                signConfig.TimeStampServer ?? "http://timestamp.globalsign.com/scripts/timstamp.dll",
                this.ValidateTimestamp)
            {
                DisplayName = "Time stamp URL"
            };

            this.Store = new ChangeableProperty<CertificateSource>(signConfig.Source);
            this.Store.ValueChanged += StoreOnValueChanged;
            this.PfxPath = new ChangeableFileProperty(interactionService, signConfig.PfxPath?.Resolved)
            {
                IsValidated = false,
                DisplayName = "Path to PFX file",
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
                        // initialPassword = crypto.DecryptString(initialPassword, "$%!!ASddahs55839AA___ąółęńśSdcvv");
                        initialPassword = crypto.Unprotect(initialPassword);
                        initialSecurePassword = new SecureString();
                        foreach (var p in initialPassword ?? string.Empty)
                        {
                            initialSecurePassword.AppendChar(p);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.Warn("The encrypted password from settings could not be decrypted.");
                    }
                }
            }

            this.Password = new ChangeableProperty<SecureString>(initialSecurePassword);

            this.SelectedPersonalCertificate = new ValidatedChangeableProperty<CertificateViewModel>(this.ValidateSelectedCertificate)
            {
                DisplayName = "Selected certificate",
                IsValidated = false
            };

            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates(signConfig.Thumbprint, !signConfig.ShowAllCertificates));
            this.ShowAllCertificates = new ChangeableProperty<bool>(signConfig.ShowAllCertificates);

            this.AddChildren(this.SelectedPersonalCertificate, this.PfxPath, this.TimeStamp, this.Password, this.Store, this.ShowAllCertificates);
            this.ShowPassword = showPassword;

            this.ShowAllCertificates.ValueChanged += async (sender, args) =>
            {
                await this.PersonalCertificates.Load(this.LoadPersonalCertificates(this.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint, !(bool)args.NewValue)).ConfigureAwait(false);
                this.OnPropertyChanged(nameof(this.SelectedPersonalCertificate));
            };

            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                this.PfxPath.IsValidated = true;
            }
            else
            {
                this.SelectedPersonalCertificate.IsValidated = true;
            }
        }

        public bool ShowPassword { get; }

        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public ValidatedChangeableProperty<CertificateViewModel> SelectedPersonalCertificate { get; }

        public ChangeableProperty<SecureString> Password { get; }

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

        public ChangeableProperty<bool> ShowAllCertificates { get; }

        private string ValidateSelectedCertificate(CertificateViewModel arg)
        {
            if (this.Store.CurrentValue == CertificateSource.Pfx)
            {
                return null;
            }

            return arg == null ? "The certificate is required." : null;
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
            this.PfxPath.IsValidated = (CertificateSource)e.NewValue == CertificateSource.Pfx;
            this.SelectedPersonalCertificate.IsValidated = (CertificateSource)e.NewValue == CertificateSource.Personal;
        }
    }
}

