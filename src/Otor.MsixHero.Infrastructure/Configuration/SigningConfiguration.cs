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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract(Name = "signing")]
    public class SigningConfiguration : BaseJsonSetting
    {
        public SigningConfiguration()
        {
            this.DefaultOutFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "Certificates");
        }

        [DataMember(Name = "defaultOutputFolder")]
        public ResolvableFolder.ResolvablePath DefaultOutFolder { get; set; }

        [DataMember(Name = "showAllCertificates")]
        public bool ShowAllCertificates { get; set; }

        public List<SigningProfile> Profiles { get; set; }

        public SigningProfile GetSelectedProfile()
        {
            if (this.Profiles == null)
            {
                return null;
            }

            return this.Profiles.FirstOrDefault(p => p.IsDefault) ?? this.Profiles.FirstOrDefault();
        }
    }
}