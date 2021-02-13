// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
    [XmlRoot("changed")]
    public class ChangedFiles
    {
        [XmlArray("oldPackage")]
        [XmlArrayItem("file")]
        public List<AppxFile> OldPackageFiles { get; set; }

        [XmlArray("newPackage")]
        [XmlArrayItem("file")]
        public List<AppxFile> NewPackageFiles { get; set; }
        
        [XmlAttribute("fileCount")]
        public long FileCount { get; set; }

        [XmlAttribute("oldPackageFileSize")]
        public long OldPackageFileSize { get; set; }

        [XmlAttribute("oldPackageBlockCount")]
        public long OldPackageBlockCount { get; set; }

        [XmlAttribute("oldPackageBlockSize")]
        public long OldPackageBlockSize { get; set; }

        [XmlAttribute("newPackageFileSize")]
        public long NewPackageFileSize { get; set; }

        [XmlAttribute("newPackageBlockCount")]
        public long NewPackageBlockCount { get; set; }

        [XmlAttribute("newPackageBlockSize")]
        public long NewPackageBlockSize { get; set; }

        [XmlAttribute("updateImpact")]
        public long UpdateImpact { get; set; }
        
        [XmlIgnore]
        public long ActualUpdateImpact { get; set; }

        [XmlAttribute("sizeDiff")]
        public long SizeDifference { get; set; }
    }
}