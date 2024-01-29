// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Entities.Appx;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public abstract class BaseFiles
    {
        [XmlElement("file")]
        public List<AppxFile> Files { get; set; }
        
        [XmlAttribute("fileSize")]
        public long FileSize { get; set; }

        [XmlAttribute("blockSize")]
        public long BlockSize { get; set; }

        [XmlAttribute("fileCount")]
        public long FileCount { get; set; }

        [XmlAttribute("blockCount")]
        public long BlockCount { get; set; }
    }
}