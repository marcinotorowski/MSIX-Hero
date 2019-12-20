using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;

namespace otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel
{
    public class CertificateSelectorViewModel : ChangeableContainer
    {
        private readonly IAppxSigningManager signingManager;

        public CertificateSelectorViewModel(
            IInteractionService interactionService, 
            IAppxSigningManager signingManager,
            SigningConfiguration configuration,
            bool showPassword)
        {
            this.signingManager = signingManager;
            var signConfig = configuration ?? new SigningConfiguration();

            this.TimeStamp = new ValidatedChangeableProperty<string>(this.ValidateTimestamp, signConfig.TimeStampServer ?? "http://timestamp.globalsign.com/scripts/timstamp.dll");
            this.Store = new ChangeableProperty<CertificateSource>(signConfig.Source );
            this.Store.ValueChanged += StoreOnValueChanged;
            this.PfxPath = new ChangeableFileProperty(interactionService, signConfig.PfxPath.Resolved) { Filter = "PFX files|*.pfx", Validators = new [] { ChangeableFileProperty.ValidatePathAndPresence }};
            this.Password = new ChangeableProperty<SecureString>();
            this.SelectedPersonalCertificate = new ValidatedChangeableProperty<CertificateViewModel>(this.ValidateSelectedCertificate);
            this.PersonalCertificates = new AsyncProperty<ObservableCollection<CertificateViewModel>>(this.LoadPersonalCertificates(signConfig.Thumbprint));

            this.AddChildren(this.SelectedPersonalCertificate, this.PfxPath, this.TimeStamp, this.Password, this.Store);
            this.IsValidated = false;
            this.ShowPassword = showPassword;
        }

        public bool ShowPassword { get; }

        public AsyncProperty<ObservableCollection<CertificateViewModel>> PersonalCertificates { get; }

        public ChangeableProperty<CertificateViewModel> SelectedPersonalCertificate { get; }

        public ChangeableProperty<SecureString> Password { get; }

        public ChangeableProperty<CertificateSource> Store { get; }

        private async Task<ObservableCollection<CertificateViewModel>> LoadPersonalCertificates(string thumbprint = null, CancellationToken cancellationToken = default)
        {
            var certs = await this.signingManager.GetCertificatesFromStore(CertificateStoreType.MachineUser, cancellationToken).ConfigureAwait(false);
            var result = new ObservableCollection<CertificateViewModel>(certs.Select(c => new CertificateViewModel(c)));
            this.SelectedPersonalCertificate.CurrentValue = string.IsNullOrEmpty(thumbprint) ? result.FirstOrDefault() : result.FirstOrDefault(c => c.Model.Thumbprint == thumbprint);
            this.SelectedPersonalCertificate.Commit();
            return result;
        }
        
        public ChangeableFileProperty PfxPath { get; }

        public ChangeableProperty<string> TimeStamp { get; }

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
            this.PfxPath.IsValidated = ((CertificateSource)e.NewValue) == CertificateSource.Pfx;
        }
    }
}

