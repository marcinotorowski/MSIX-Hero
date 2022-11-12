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

using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class AppAttachConfiguration : BaseJsonSetting
    {
        public AppAttachConfiguration()
        {
            this.ExtractCertificate = true;
            this.GenerateScripts = true;
            this.JunctionPoint = "C:\\temp\\msix-app-attach";
            this.UseMsixMgrForVhdCreation = true;
        }

        [DataMember(Name = "useMsixMgrForVhdCreation")]
        public bool UseMsixMgrForVhdCreation { get; set; }

        [DataMember(Name = "junctionPoint")]
        public string JunctionPoint { get; set; }

        [DataMember(Name = "extractCertificate")]
        public bool ExtractCertificate { get; set; }

        [DataMember(Name = "generateScripts")]
        public bool GenerateScripts { get; set; }
    }
}