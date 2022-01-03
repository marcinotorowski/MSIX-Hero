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
