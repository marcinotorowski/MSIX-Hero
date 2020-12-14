using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Signing.Entities;

namespace Otor.MsixHero.App.Controls.CertificateSelector.ViewModel
{
    public class CertificateViewModel : NotifyPropertyChanged
    {
        private readonly PersonalCertificate personalCertificate;

        public CertificateViewModel(PersonalCertificate personalCertificate)
        {
            this.personalCertificate = personalCertificate;
        }

        public string DisplayName => string.IsNullOrEmpty(this.personalCertificate.DisplayName) ? this.personalCertificate.Subject : this.personalCertificate.DisplayName;

        public string Issuer => string.IsNullOrEmpty(this.personalCertificate.Issuer) ? this.personalCertificate.Subject : this.personalCertificate.Issuer;

        public string Subject => this.personalCertificate.Subject;

        public string DigestAlgorithm => this.personalCertificate.DigestAlgorithm;
        
        public string Thumbprint => this.personalCertificate.Thumbprint;

        public CertificateStoreType StoreType => this.personalCertificate.StoreType;

        public PersonalCertificate Model => this.personalCertificate;
    }
}
