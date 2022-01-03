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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;

namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto
{
    public class CreateVolumeDto : ProxyObject
    {
        public CreateVolumeDto(IEnumerable<string> packagePath, string vhdDirectory)
        {
            this.Packages = packagePath.ToArray();
            this.VhdDirectory = vhdDirectory;
        }

        public CreateVolumeDto(string packagePath, string vhdFilePath)
        {
            this.Packages = new[] { packagePath };
            this.VhdDirectory = Path.GetDirectoryName(vhdFilePath);
            this.VhdName = Path.GetFileName(vhdFilePath);
        }

        public CreateVolumeDto()
        {
        }

        public string[] Packages { get; set; }

        public string VhdDirectory { get; set; }
        
        public string VhdName { get; set; }
        
        public uint SizeInMegaBytes { get; set; }
        
        public AppAttachVolumeType Type { get; set; }

        public bool GenerateScripts { get; set; }

        public bool ExtractCertificate { get; set; }
    }
}
