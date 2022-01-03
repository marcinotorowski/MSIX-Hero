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
    public class RunToolInContextDto : ProxyObject
    {
        public RunToolInContextDto(string packagePackageFamilyName, string packageName, string toolPath, string arguments = null)
        {
            this.PackageFamilyName = packagePackageFamilyName;
            this.AppId = packageName;
            this.ToolPath = toolPath;
            this.Arguments = arguments;
        }

        public RunToolInContextDto(InstalledPackage package, string toolPath, string arguments = null) : this(package.PackageFamilyName, package.Name, toolPath, arguments)
        {
        }

        public RunToolInContextDto()
        {
        }

        [XmlElement]
        public string PackageFamilyName { get; set; }

        [XmlElement]
        public string AppId { get; set; }
        
        [XmlElement]
        public string ToolPath { get; set; }
        
        [XmlElement]
        public string Arguments { get; set; }
    }
}
