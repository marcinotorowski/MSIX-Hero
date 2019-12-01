using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel
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

        public CertificateStoreType StoreType => this.personalCertificate.StoreType;

        public PersonalCertificate Model => this.personalCertificate;
    }
}
