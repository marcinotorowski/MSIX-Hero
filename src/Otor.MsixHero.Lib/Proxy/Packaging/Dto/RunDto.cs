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
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class RunDto : ProxyObject
    {
        public RunDto()
        {
        }

        public RunDto(string manifestPath, string applicationId = null)
        {
            this.ManifestPath = manifestPath;
            this.ApplicationId = applicationId;
        }

        public RunDto(InstalledPackage package, string applicationId = null) : this(package.ManifestLocation, applicationId)
        {
        }
        
        [XmlElement]
        public string ManifestPath { get; set; }

        [XmlElement]
        public string ApplicationId { get; set; }
    }
}
