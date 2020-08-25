using System;

namespace Otor.MsixHero.Appx.Signing.Entities
{
    [Serializable]
    public class PersonalCertificate
    {
        public string Issuer { get; set; }

        public DateTime? Date { get; set; }

        public string DisplayName { get; set; }

        public string Subject { get; set; }

        public string Thumbprint { get; set; }

        public CertificateStoreType StoreType { get; set; }

        public string DigestAlgorithm { get; set; }
    }
}
