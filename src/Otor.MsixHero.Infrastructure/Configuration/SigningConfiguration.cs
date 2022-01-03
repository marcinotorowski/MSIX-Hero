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
using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract(Name = "signing")]
    public class SigningConfiguration : BaseJsonSetting
    {
        public SigningConfiguration()
        {
            this.Source = CertificateSource.Unknown;
            this.DefaultOutFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "Certificates");
            this.TimeStampServer = "http://timestamp.globalsign.com/scripts/timstamp.dll";
            this.DeviceGuard = new DeviceGuardConfiguration();
        }

        [DataMember(Name = "defaultOutputFolder")]
        public ResolvableFolder.ResolvablePath DefaultOutFolder { get; set; }

        [DataMember(Name="timeStampServer")]
        public string TimeStampServer { get; set; }

        [DataMember(Name = "source")]
        public CertificateSource Source { get; set; }

        [DataMember(Name = "thumbprint")]
        public string Thumbprint { get; set; }

        [DataMember(Name = "showAllCertificates")]
        public bool ShowAllCertificates { get; set; }

        [DataMember(Name = "pfx")]
        public ResolvableFolder.ResolvablePath PfxPath { get; set; }

        [DataMember(Name = "encodedPassword")]
        public string EncodedPassword { get; set; }

        [DataMember(Name = "deviceGuard")]
        public DeviceGuardConfiguration DeviceGuard { get; set; }
    }
}